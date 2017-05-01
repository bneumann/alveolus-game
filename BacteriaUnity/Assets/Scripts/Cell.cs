using System;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cell : MonoBehaviour
    {
        private ModelParameter mParameter;
        private GameController mGameController;
        private ParticleSystem mChemokineEmitter;
        public int BacteriaOnCell;
        // Cell width is mParameter.EpithelialCellWidth (30)

        public void Start()
        {
            mGameController = GameObject.Find("GameController").GetComponent<GameController>();
            mChemokineEmitter = GetComponent<ParticleSystem>();

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

        public float Chemokine
        {
            set
            {
                var input = Mathf.Clamp01(value * 1e3F);
                if(mChemokineEmitter)
                {
                    var m = mChemokineEmitter.main;
                    m.maxParticles = (int)input*1000;
                }
            }
        }
    }
}
