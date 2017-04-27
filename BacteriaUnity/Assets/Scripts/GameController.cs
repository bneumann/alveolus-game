using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
	{
		// Defaults and statics
		//int output = 0;     // initial number of bacteria spread
		//int n = 0;          // Iterations
		//int nb;             // Initial number of bacteria
		//int Nc = 11;        // Number of cells per row in alveolus
		//int cel = 30;       // Width of epitheliaol cell in microm
		//int nummac;         // Number of macrophages, approximation from Bionumbers
		//int nstop = 0;      // Counting stoping time between the models
		//int foco = 4;       // Dispersion of initial bacteria in the alveolus
		// Generated properties
		private int mIteration = 0;

        public Vector3 ScreenSize { get { return new Vector3(Screen.width, Screen.height, 0); } }

		public float Width { get { return Camera.main.ScreenToWorldPoint(ScreenSize).x; } }
		public float Height { get { return Camera.main.ScreenToWorldPoint(ScreenSize).y; } }

        public GameObject bacteria;
        public ModelParameter Parameter;

        private List<GameObject> bactList = new List<GameObject>();

		public void Start()
		{
			Parameter = new ModelParameter() { /*EpithelialCellsPerRow = 40,*/ BacteriaDoublingTime = 1};

            //worldObjects.Add(new Cell(mParameter));

            //for (int i = 0; i < mParameter.NumberOfMacrophages; i++)
            //{
            //	Macrophage m = new Macrophage(mParameter);
            //	worldObjects.Add(m);
            //}

            for (int nb = 0; nb < Parameter.NumberOfBacteria; nb++)
			{
                var camera = Camera.main;
                Vector3 spawnPosition = new Vector3(Random.Range(-10, 10), Random.Range(-5, 5), 0);
                Quaternion spawnRotation = Quaternion.identity;
                bactList.Add(Instantiate(bacteria, spawnPosition, spawnRotation));
				//worldObjects.Add(new Bacteria(mParameter));
			}
		}

		public void Update()
		{
            //TODO: Use methods shown in: https://www.youtube.com/watch?v=r8N6J79W0go
            if (mIteration % (Parameter.BacteriaDoublingTime) == 0)
            {
                //	var bactList = worldObjects.Where(wo => wo.GetType() == typeof(Bacteria)).ToArray();
                var tmpList = new List<GameObject>();
                for (int i = 0; i < bactList.Count; i++)
                {
                    var b = bactList[i];
                    //		//worldObjects.Add(new Bacteria(mParameter) { X = b.X, Y = b.Y });
                    Vector3 spawnPosition = new Vector3(b.transform.position.x, b.transform.position.y);
                    Quaternion spawnRotation = Quaternion.identity;
                    tmpList.Add(Instantiate(bacteria, spawnPosition, spawnRotation));
                }
                bactList.AddRange(tmpList);

            }
            mIteration++;
        }

	}

}
