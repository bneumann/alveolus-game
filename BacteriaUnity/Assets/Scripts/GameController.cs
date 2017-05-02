using System.Collections;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
	{
        public Vector3 ScreenSize { get { return new Vector3(Screen.width, Screen.height, 0); } }

		public float Width { get { return Camera.main.ScreenToWorldPoint(ScreenSize).x; } }
		public float Height { get { return Camera.main.ScreenToWorldPoint(ScreenSize).y; } }

        public int MacrophageCount { get { return FindObjectsOfType(typeof(Macrophage)).Length; } }
        public int BacteriaCount { get { return FindObjectsOfType(typeof(Bacteria)).Length; } }
        public Bacteria[] Bacterias { get { return FindObjectsOfType(typeof(Bacteria)) as Bacteria[]; } }

        public GameObject bacteria;
        public GameObject macrophage;
        public ModelParameter Parameter = new ModelParameter() { /*EpithelialCellsPerRow = 40,*/ BacteriaDoublingTime = 20, NumberOfBacteria = 100 };

        public int NumberOfBacteria;
        public int NumberOfMacrophages;

        public void Start()
		{
            Parameter.NumberOfBacteria = NumberOfBacteria;
            Parameter.NumberOfMacrophages = NumberOfMacrophages;

            Vector3 spawnPosition;
            Quaternion spawnRotation = Quaternion.identity;

            for (int i = 0; i < Parameter.NumberOfMacrophages; i++)
            {
                spawnPosition = new Vector3(Random.Range(0, Width), Random.Range(0, Height), 0);
                Instantiate(macrophage, spawnPosition, spawnRotation);
            }

            for (int nb = 0; nb < Parameter.NumberOfBacteria; nb++)
			{
                var x = SampleGaussian(0, Width / 4);
                var y = SampleGaussian(0, Height / 4);
                spawnPosition = new Vector3(x, y, 0);            
                GameObject bact = Instantiate(bacteria, spawnPosition, spawnRotation);
                bact.transform.parent = GameObject.FindGameObjectWithTag("Bacterias").transform;
            }

			StartCoroutine(DoubleBacteria());
        }

		public void Update()
		{
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        /// <summary>
        /// Double bacterias every Parameter.BacteriaDoublingTime seconds. See Modelparameters to change
        /// </summary>
        /// <returns>IEnumerator object</returns>
		IEnumerator DoubleBacteria()
		{
			while (true)
			{
				Bacteria[] bactList = FindObjectsOfType(typeof(Bacteria)) as Bacteria[];
				for (int i = 0; i < bactList.Length; i++)
				{
					var b = bactList[i];
					Vector3 spawnPosition = new Vector3(b.transform.position.x, b.transform.position.y);
					Quaternion spawnRotation = Quaternion.identity;
                    var bact = Instantiate(bacteria, spawnPosition, spawnRotation);
                    bact.transform.parent = GameObject.FindGameObjectWithTag("Bacterias").transform;
                }
				yield return new WaitForSeconds(Parameter.BacteriaDoublingTime);
			}
		}

        private static float SampleGaussian(float mean, float stddev)
        {
            // The method requires sampling from a uniform random of (0,1]
            // but Random.NextDouble() returns a sample of [0,1).
            float x1 = 1 - Random.Range(0F, 1F);
            float x2 = 1 - Random.Range(0F, 1F);

            float y1 = Mathf.Sqrt(-2.0F * Mathf.Log(x1)) * Mathf.Cos(2.0F * Mathf.PI * x2);
            return y1 * stddev + mean;
        }

    }

}
