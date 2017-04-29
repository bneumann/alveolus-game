using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cell : MonoBehaviour
    {
        private ModelParameter mParameter;
        private GameController mGameController;
        public int BacteriaOnCell;
        // Cell width is mParameter.EpithelialCellWidth (30)

        public void Start()
        {
            mGameController = GameObject.Find("GameController").GetComponent<GameController>();
            mParameter = mGameController.Parameter;
        }

        private void OnTriggerEnter2D(Collider2D e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                BacteriaOnCell++;
            }
        }
        private void OnTriggerExit2D(Collider2D e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                BacteriaOnCell--;
            }
        }

        public void Update()
        {
        }

        //public int BacteriaOnCell
        //{
        //    get
        //    {
        //        return mGameController.Bacterias.Count(b => Vector3.Distance(transform.position, b.transform.position) < 1.1F); // mParameter.EpithelialCellWidth);
        //    }
        //}

    }
}
