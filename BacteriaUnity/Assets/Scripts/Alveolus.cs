using System.Collections;
using System.Linq;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using Assets.Scripts;

public class Alveolus : MonoBehaviour
{
    private ModelParameter mParameter;

    private Matrix<double> CellMatrix;

    // Object used for generating the tile map
    public GameObject cell;

    //Size you want your tile in unity units
    public float cellTileWidth = 1.5F;
    public float cellTileHeight = 1.11F;

    //Tilemap width and height
    public int cellHorizontalCount = 13;
    public int cellVerticalCount = 9;

    // Cell Model for chemokine generation
    //global contador matBac
    //contador = contador + 1; %Count the iterations
    //%Parameters
    //x(x< 1e-6) = 0;
    //R           = cel* N; %Total length of surface, microm
    //dx          = R/N; %Thickness of each layer microm
    //NN = N * N; %Number of cells
    //X1 = vec2mat(x(1:NN), N)';
    //k1          = 4e-4; %Abitrary parameter
    //k2          = 1e3; %Abitrary parameter
    //Di          = 6e3* pars(3); %Abitrary parameter
    private float SurfaceLength { get { return mParameter.EpithelialCellWidth * mParameter.EpithelialCellsPerRow; } }
    private float CellLayerThickness { get { return SurfaceLength / mParameter.EpithelialCellsPerRow; } }
    private int NumberOfCells { get { return mParameter.EpithelialCellsPerRow * mParameter.EpithelialCellsPerRow; } }

    private const float k1 = 4e-4F; // Arbitrary parameter
    private const float k2 = 1e3F; // Arbitrary parameter
    private float Di { get { return mParameter.ChemokineDiffusionConstant; } }

    //2D array to hold all tiles, which makes it easier to reference adjacent tiles etc.
    public GameObject[,] map;
    private Matrix<double> mBactCountMatrix;

    public int MacrophageCount;

    // Use this for initialization
    void Start ()
    {
        if(!cell)
            throw new System.Exception("Cell not found!!!");
        GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
        mParameter = gc.Parameter;

        // Overwrite parameter from unity to match screen
        mParameter.EpithelialCellsPerRow = cellHorizontalCount;
        mParameter.EpithelialCellsPerColumn = cellVerticalCount;

        CellMatrix = Matrix<double>.Build.Dense(mParameter.EpithelialCellsPerRow, mParameter.EpithelialCellsPerColumn, 0);

        //TODO: Also move to Alveolus class
        //Initialize our 2D Transform array with the width and height
        map = new GameObject[cellHorizontalCount, cellVerticalCount];

        //Iterate over each future tile positions for x and y
        for (int y = 0; y < cellVerticalCount; y++)
        {
            for (int x = 0; x < cellHorizontalCount; x++)
            {
                //Instantiate tile prefab at the desired position as a Transform object
                var offset = y % 2 == 0 ? cellTileWidth / 2 : 0;
                var xCoord = x * cellTileWidth - gc.Width - offset;
                var yCoord = y * cellTileHeight - gc.Height;
                Transform tile = Instantiate(cell.transform, new Vector3(xCoord, yCoord, 0), Quaternion.identity) as Transform;
                //Set the tiles parent to the GameObject this script is attached to
                tile.parent = transform;
                //Set the 2D map array element to the current tile that we just created.
                map[x, y] = tile.gameObject;
            }
        }

        mBactCountMatrix = Matrix<float>.Build.Dense(map.GetLength(0), map.GetLength(1));

        StartCoroutine(CalculateChemokine());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator CalculateChemokine()
    {
        while (true)
        {
            var ret = SetEquations();
            SolveEquations(ret);
            yield return new WaitForSeconds(mParameter.BacteriaDoublingTime);
        }
    }

    private void SolveEquations(Vector<double> initials)
    {
        var ret = MathNet.Numerics.OdeSolvers.RungeKutta.SecondOrder(initials, 0, 100, initials.Count, null);
    }

    //%Spatial model
    // zero = zeros(1, N);
    // % 1. Fluxes in x-direction; zero fluxes near boundaries
    // FI = -Di.*[zero;X1(2:N,:) - X1(1:N-1,:); zero]./dx;
    // % Add flux gradient to rate of change
    // dI = (FI(2:N + 1,:) - FI(1:N,:))./ dx;
    // % 2. Fluxes in y-direction; zero fluxes near boundaries
    // FI = -Di.*[zero',X1(:,2:N) - X1(:,1:N-1),zero']./dx;
    // % Add flux gradient to rate of change
    // dI = -dI - (FI(:, 2:N + 1) - FI(:, 1:N))./ dx;
    //%%
    //dxX = k1.* matBac.*pars(4) - k2.* X1.^2 + dI;
    //dfx = dxX(:); %Me ge equations
    private Vector<double> SetEquations()
    {
        int rows = mParameter.EpithelialCellsPerRow;
        int cols = mParameter.EpithelialCellsPerColumn;
        //1. Fluxes in x-direction; zero fluxes near boundaries
        var zero = Matrix<double>.Build.Dense(1, cols, 0);
        var diff = CellMatrix.SubMatrix(1, rows - 1, 0, CellMatrix.ColumnCount) - CellMatrix.SubMatrix(0, rows - 1, 0, CellMatrix.ColumnCount);
        var FI = zero.Stack(diff);
        FI = -Di * FI.Stack(zero) / CellLayerThickness;
        // Add flux gradient to rate of change
        var dI = FI.SubMatrix(1, rows, 0, FI.ColumnCount) - FI.SubMatrix(0, rows, 0, FI.ColumnCount) / CellLayerThickness;
        //2. Fluxes in y-direction; zero fluxes near boundaries
        zero = Matrix<double>.Build.Dense(rows, 1, 0);
        diff = CellMatrix.SubMatrix(0, CellMatrix.RowCount, 1, cols - 1) - CellMatrix.SubMatrix(0, CellMatrix.RowCount, 0, cols - 1);
        FI = zero.Append(diff);
        FI = -Di * FI.Append(zero) / CellLayerThickness;
        // Add flux gradient to rate of change
        //  dI = -dI - (FI(:, 2:N + 1) - FI(:, 1:N))./ dx;
        dI = -dI - (FI.SubMatrix(0, FI.RowCount, 1, cols) - FI.SubMatrix(0, FI.RowCount, 0, cols)) / CellLayerThickness;
        //FIXME: Macrophagecount should be the cellmatrix containing numbers of bacteria attached
        var dxX = k1 * BactCount * mParameter.AntigenPerBacteria - k2 * CellMatrix.PointwiseMultiply(CellMatrix) + dI;
        //Debug.Log(FI);
        //Debug.Log(dI);
        Debug.Log(dxX);

        var ret = Vector<double>.Build.Dense(rows * cols, 1);
        for(int i = 0; i < dxX.ColumnCount; i++)
        {
            ret.SetSubVector(i * rows, rows, dxX.Column(i));
        }
        return ret;
    }

    private Matrix<double> BactCount {
        get
        {
            for (int row = 0; row < mBactCountMatrix.RowCount; row++)
            {
                for (int col = 0; col < mBactCountMatrix.ColumnCount; col++)
                {
                    Cell curCell = map[row, col].GetComponent<Cell>();
                    mBactCountMatrix[row, col] = curCell.BacteriaOnCell;
                }
            }
            return mBactCountMatrix;
        }
    }
}
