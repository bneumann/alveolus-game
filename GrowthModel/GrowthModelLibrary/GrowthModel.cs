using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;
using static GrowthModelLibrary.MatlabWrapper;

namespace GrowthModelLibrary
{
    /// <summary> 
    /// Parameter for the Growth Model
    /// 1.number of bacteria (1),
    /// 2.number of macrophages (3), 
    /// 3.chemokine diffusion constant (6e3),
    /// 4.antigens per bacteria (1),
    /// 5.Movement in sessile phase (0.1),
    /// 6.Movement in flowing phase (3),
    /// 7.Radial flow (4.2e-2),
    /// 8.Bacteria dobling time (200),
    /// 9.Probability interchanged (0.999),
    /// 10.Bacteria saturation number (10),
    /// 11.Macrophage movement (2),
    /// 12.Phagocytosys rate (0.048),
    /// 13.Maximum bacteria per macrophage (50)
    /// 14.Distance to sense the metabolic gradient (30)
    /// 15.Sensitivity to feel the cytokine gradient (1e-6)]
    /// </summary>
    public class ModelParameter 
    {
        public int NumberOfBacteria                     = 1;
        public int NumberOfMacrophages                  = 3;
        public int ChemokineDiffusionCOnstant           = 6000;
        public int AntigenPerBacteria                   = 1;
        public double MovementInSessilePhase            = 0.1f;
        public double MovementInFlowingPhase            = 3f;
        public double RadialFlow                        = 4.2e-2;
        public int BacteriaDoublingTime                 = 200;
        public double ProbabilityInterchanged           = 0.999;
        public int BacteriaSaturationNumber             = 10;
        public int MacrophageMovement                   = 2;
        public double PhagocytosysRate                  = 0.048;
        public int MaximumBacteriaPerMacrophage         = 50;
        public int DistanceToSenseMetabolicGradient     = 30;
        public double SensitivityToFeelCytokineGradient = 1e-6;
    }

    public class MacrophageParameter
    {
        // Biological parameters for macrophages
        int macsiz = 10; // Macrophage size in micras (21 microm of diameter, from Wikipedia)
        int stepm;
        double clebac;
        int maxeat;
        double sensit;
        Matrix<double> indMac;

        public MacrophageParameter(ModelParameter parameter, int NumberOfMacrophages)
        {
            stepm = 2 * parameter.MacrophageMovement; // Movement of macrophages (2 micrm/min de [[PMID: 26202827]])
            clebac = 0.048 * parameter.PhagocytosysRate; // Phagocytosys rate (min-1, calculated in Excel parameters calculation.xlsx (sheet: Phagocytosys rate) [[PMC266186]])
            maxeat = 50 * parameter.MaximumBacteriaPerMacrophage; // Maximum number of bacteria attached [[buscar]] // // //  More than 70//  of the macrophage perimeter covered of bacteria inside one minute
            sensit = 1e-6 * parameter.SensitivityToFeelCytokineGradient; // Sensitivity to feel the cytokine gradient [[buscar]].
            indMac = ones(NumberOfMacrophages, 2); // Initial value for indexes of macrophages
        }
    }

    public class Dimension
    {
        public int Xmin;
        public int Xmax;
        public int Ymin;
        public int Ymax;
    }

    public class Position
    {
        public int X;
        public int Y;

        public override string ToString()
        {
            return String.Format("X: {0}, Y: {1}", X, Y);
        }
    }

    public class Model
    {
        // Defaults and statics
        int output = 0;     // initial number of bacteria spread
        int n = 0;          // Iterations
        int nb;             // Initial number of bacteria
        int Nc = 11;        // Number of cells per row in alveolus
        int cel = 30;       // Width of epitheliaol cell in microm
        int nummac;         // Number of macrophages, approximation from Bionumbers
        int nstop = 0;      // Counting stoping time between the models
        int foco = 4;       // Dispersion of initial bacteria in the alveolus
        //int writerObj1;     // Create a movie
        //int movie;
        //int final;
        //int pars;
        Matrix<double> step; // Movement rate in the sesile phase(step1)
        double step1;       // Movement rate in the sesile phase, assumed
        double step2;       // Movemenet rate in the mobile phase, assumed
        double step3;       // min-1. Radial flow(4.2e-2) Fitted to alveolar clearance in figura 3B[[PMID: 17290033]] calculated in Excel parameters calculation.xlsx(sheet: alveoli flow) 
        //int pantalla;

        // Generated properties
        Matrix<double> neat;        // Intrinsic macrophage time for bacteria phagocytosis
        Matrix<double> eaten;       // Register of eated bacteria
        int dimens;                 // Dimension of the alveoli in micrometers from Bionumbers (11 from 121 cells)
        Matrix<double> position;    // Normal distribution of initial bacteria
        Matrix<double> mac;         // Random localization of macrophages
        // FIXME: Is actually a list that will be increased over time:
        Matrix<double> Chemokine;   // Chemokine 

        //Biological parameters for bacteria
        double growth;                  // min-1. 199.2 min doubling time (3.32 hours from [[pag 38 book: ISBN 978-1-908230-17-1]])Iterations per division (5e2)
        double probS;                   // Probability of not being interchanged between phases (0.999) A lower value stablish the infection with low probability 
        double probC = 0.995;           // Probability of not leave the alveolus from the mobile phase
        double maxbac = 1e3;            // Maximum number of bacteria in the alveolus simulated, not relevant
        double rad = 1.5;               // Region for determine the saturation of bacteria (1.5) Assuming two hexagonal layers
        double Mbac;                    // Maximum number of bacteria inside the region to saturate the growth (10) Assuming two hexagonal layers
        Matrix<double> matBac;          // Define the bacteria matrix

        // Internal status variables
        private ModelParameter ModelParameter = null;
        private MacrophageParameter macroParameter = null;


        // New class descriptions
        // Setter / Getter
        private bool mCough = false;
        public bool Cough { set { mCough = value; } }

        public Model(ModelParameter parameter)
        {
            /**
             * Initialize parameters that are calculated
             **/
            this.ModelParameter = parameter;
            // TODO: Check if parameters might accidently always been 1 and not correctly checked in MATLAB
            nb = parameter.NumberOfBacteria;
            nummac = parameter.NumberOfMacrophages;
            growth = Math.Round((double)(200 * parameter.BacteriaDoublingTime));
            probS = 0.999 * parameter.ProbabilityInterchanged;
            Mbac = 10 * parameter.BacteriaSaturationNumber; // Matlab returns 10 not 100
            matBac = zeros(Nc, Nc);

            dimens = (cel * Nc) / 2;
            position = random(0, (int)dimens / foco, (int)nb, 2);

            mac = dimens * (rand(nummac, 2) * 2 - 1);
            Chemokine = zeros(Nc, Nc); // originally: zeros(Nc,Nc,1);
            eaten = zeros(1, nummac);
            neat = zeros(1, nummac);

            // One time non model stuff?!
            int[] limits = new int[] { -dimens, dimens }; // Dimension of alveolus in nm (30 microM/cell [[PMID: 25360787 & PMID: 7611375]])
            position = position.Map(p => p > dimens ? dimens : p);
            position = position.Map(p => p < -dimens ? -dimens : p);
            double[] front = linspace(limits[0], limits[1], Nc + 1); // Define the frontiers of the epithelial cells

            step3 = 4.2e-2 * parameter.RadialFlow;

            step2 = 3 * parameter.MovementInFlowingPhase;
            step1 = 0.1 * parameter.MovementInSessilePhase;
            step = ones(nb, 1) * step1;

            Infection i = new Infection(parameter);
            i.Grow();

            macroParameter = new MacrophageParameter(ModelParameter, nummac);
        }

        public void grow()
        {
            // Bacterial saturable growth
            var sqdi = squareform(pdist(position)); // Calculate the distances
            var sqce = zeros(sqdi.RowCount, sqdi.ColumnCount);
            sqce = sqdi.Map(p => p < rad ? 1.0 : 0.0); // Find short distances
            // FIXME: Check if equivalent to: sum(sqce, 2); -> MATLAB: if A is a matrix, then sum(A,2) is a column vector containing the sum of each row.
            Vector<double> nearestBacteria = sqce.RowSums(); // Count the close bacteria
            Vector<double> bacteriaToGrow = nearestBacteria.Map(p => p < Mbac ? 0 : p); // Define the growing bacteria below the threshold

            if((n % growth) == 0)
            {
                int bacteriaGrowSum = (int)Math.Round(bacteriaToGrow.Sum());
                for(int iter = nb; iter < nb + bacteriaGrowSum; iter++)
                {
                    position.SetColumn(iter, bacteriaToGrow);
                }
                step.SetColumn(0, bacteriaToGrow);
                //position(nb + 1:(nb + bacteriaGrowSum),:) = position(bacteriaToGrow,:); // Bacteria divide
                //step(nb + 1:(nb + bacteriaGrowSum), 1) = step(bacteriaToGrow, 1); // Doubling the step vector
                nb = nb + bacteriaGrowSum; // Doubling the number of bacteria
            }
        }
    }       

    public class Infection : List<Bacteria>
    {
        private int mIteration = 0;
        private int mGrowthRate = 0; // See Modelparameter BacteriaDoublingTime
        private double mSaturationRadius = 1.5; // Region for determine the saturation of bacteria (1.5) Assuming two hexagonal layers
        private int mBacteriaSaturation; //See Modelparameter BacteriaSaturationNumber
        private ModelParameter mParameter;
        private static Random mRandom = new Random();

        public Infection(ModelParameter parameter)
        {
            mParameter = parameter;
            mBacteriaSaturation = 10 * mParameter.BacteriaSaturationNumber;
            mGrowthRate = (int)Math.Round((double)(200 * mParameter.BacteriaDoublingTime));

            for (int nb = 0; nb < parameter.NumberOfBacteria; nb++)
            {
                this.Add(new Bacteria(parameter));
            }
        }

        public void Cough()
        {
            var probC = 0.995;
            var rand = mRandom.NextDouble();
            foreach(Bacteria b in this)
            {
                if(mRandom.NextDouble() > probC && b.State == Bacteria.MovementStates.FlowingState)
                {
                    this.Remove(b);
                }
            }
        }

        public void Grow()
        {
            if(mIteration % mGrowthRate == 0)
            {
                var collection = new HashSet<Bacteria>();
                foreach(Bacteria ba in this)
                {
                    var tmpCollection = new HashSet<Bacteria>();
                    foreach (Bacteria bb in this)
                    {
                        // add all bacteria that are in range and not the bacteria itself
                        if(ba.GetDistance(bb) < mSaturationRadius) // !ba.Equals(bb) 
                        {
                            tmpCollection.Add(ba);
                        }
                    }
                    // If we don't have too many neighbours we can grow
                    if (tmpCollection.Count < mBacteriaSaturation)
                    {
                        tmpCollection.ToList().ForEach(b => collection.Add(b));
                    }
                }
                foreach(Bacteria b in collection)
                {
                    this.Add(new Bacteria(mParameter) { X = b.X, Y = b.Y });
                }
                if(this.Count() == 1)
                {
                    this.Add(new Bacteria(mParameter) { X = this.First().X, Y = this.First().Y });
                }                
            }
            this.ForEach(b => b.Move());
            mIteration++;
        }
    }

    public class Bacteria
    {
        public Dimension Dimension;
        private Position mPosition = new Position();
        private ModelParameter mParameter;
        private MovementStates mMovementState = MovementStates.SessileState;
        private static Random mRandom = new Random();

        public int X
        {
            get { return this.mPosition.X; }
            set
            {
                if (value > Dimension.Xmax)
                {
                    mPosition.X = Dimension.Xmax;
                }
                if (value < Dimension.Xmin)
                {
                    mPosition.X = Dimension.Xmin;
                }
                else
                {
                    mPosition.X = value;
                }
            }
        }
        public int Y
        {
            get { return this.mPosition.Y; }
            set
            {
                if (value > Dimension.Ymax)
                {
                    mPosition.Y = Dimension.Ymax;
                }
                else if (value < Dimension.Ymin)
                {
                    mPosition.Y = Dimension.Ymin;
                }
                else
                {
                    mPosition.Y = value;
                }
            }
        }
        public double StepSize
        {
            get
            {
                double step = 0;
                if (mMovementState == MovementStates.FlowingState)
                {
                    step = 3 * mParameter.MovementInFlowingPhase;
                }
                else
                {
                    step = 0.1 * mParameter.MovementInSessilePhase;
                }
                return step;
            }
        }
        public MovementStates State { get { return this.mMovementState; } }

        public enum MovementStates
        {
            SessileState,
            FlowingState
        }

        public Bacteria(ModelParameter parameter, int dimens = 165)
        {
            mParameter = parameter;
            this.Dimension = new Dimension { Xmin = -dimens, Xmax = dimens, Ymin = -dimens, Ymax = dimens };
            this.X = (int)(mRandom.NextDouble() * (double)(2 * dimens) - (double)dimens);
            this.Y = (int)(mRandom.NextDouble() * (double)(2 * dimens) - (double)dimens);
        }

        private void InterchangePhase()
        {
            // FIXME: Question: the matlab code triggers the propability 2 times. Is that wanted?
            var probS = 0.999 * mParameter.ProbabilityInterchanged;
            var randNum = mRandom.NextDouble();
            // Change state if probability is met. This will change the step size as well
            if (randNum > probS)
            {
                if (mMovementState == MovementStates.FlowingState)
                {
                    mMovementState = MovementStates.SessileState;
                }
                else
                {
                    mMovementState = MovementStates.FlowingState;
                }
            }
        }

        /// <summary>
        /// Moves the bacteria. It will jump to mobile phase with a certain propability
        /// </summary>
        public void Move()
        {
            InterchangePhase();
            InterchangePhase();
            var angl = mRandom.NextDouble() * 2 * Math.PI; // Random direction
            X += (int)(Math.Cos(angl) * StepSize);
            Y += (int)(Math.Sin(angl) * StepSize); // Step into the direction defined
        }
       
        /// <summary>
        /// Get the distance between bacterias
        /// </summary>
        /// <param name="bacteria">Other bacteria to be measured</param>
        /// <returns>Distance</returns>
        public double GetDistance(Bacteria bacteria)
        {
            return Math.Sqrt(Math.Pow(this.X - bacteria.X, 2) + Math.Pow(this.Y - bacteria.Y, 2));
        }

        public override string ToString()
        {
            return String.Format("Bacteria at {0}|{1} in {2}\n", X, Y, Enum.GetName(typeof(MovementStates), mMovementState));
        }
    }
}
