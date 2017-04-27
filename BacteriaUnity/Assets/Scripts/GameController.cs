using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
	{
        public Vector3 ScreenSize { get { return new Vector3(Screen.width, Screen.height, 0); } }

		public float Width { get { return Camera.main.ScreenToWorldPoint(ScreenSize).x; } }
		public float Height { get { return Camera.main.ScreenToWorldPoint(ScreenSize).y; } }

        public GameObject bacteria;
        public ModelParameter Parameter;

		public void Start()
		{
			Parameter = new ModelParameter() { /*EpithelialCellsPerRow = 40,*/ BacteriaDoublingTime = 10};

            //worldObjects.Add(new Cell(mParameter));

            //for (int i = 0; i < mParameter.NumberOfMacrophages; i++)
            //{
            //	Macrophage m = new Macrophage(mParameter);
            //	worldObjects.Add(m);
            //}

            for (int nb = 0; nb < Parameter.NumberOfBacteria; nb++)
			{
                Vector3 spawnPosition = new Vector3(Random.Range(-Width, Width), Random.Range(-Height, Height), 0);
                Quaternion spawnRotation = Quaternion.identity;                
                Instantiate(bacteria, spawnPosition, spawnRotation);
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
					Instantiate(bacteria, spawnPosition, spawnRotation);
				}
				yield return new WaitForSeconds(Parameter.BacteriaDoublingTime);
			}
		}

	}

}
