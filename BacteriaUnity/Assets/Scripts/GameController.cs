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
        public GameObject alveolus;
        public ModelParameter Parameter = new ModelParameter() { /*EpithelialCellsPerRow = 40,*/ BacteriaDoublingTime = 10 };


        public void Start()
		{
            Instantiate(alveolus);

            //for (int i = 0; i < mParameter.NumberOfMacrophages; i++)
            //{
            //	Macrophage m = new Macrophage(mParameter);
            //	worldObjects.Add(m);
            //}

            for (int nb = 0; nb < Parameter.NumberOfBacteria; nb++)
			{
                Vector3 spawnPosition = new Vector3(Random.Range(-Width, Width), Random.Range(-Height, Height), 0);
                Quaternion spawnRotation = Quaternion.identity;                
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

    }

}
