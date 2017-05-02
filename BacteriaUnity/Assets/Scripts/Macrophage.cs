using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
	public class Macrophage : MonoBehaviour
	{
		//private int mEatenBacteria = 0;
		// Biological parameters for macrophages
		//int macsiz = 10; // Macrophage size in micras (21 microm of diameter, from Wikipedia)
        private Vector2 mDirection = Vector2.one;
        private GameObject target;

        enum MovementStates
        {
            Idle,            // If not sensing chemokine or bacterias are close, we can idle around doing macrophagy things
            ChemokineFound,  // Somethings wrong in the neighbourhood, we should check it out
            BaceriaInRange   // EXTERMINATE!!! EXTERMINAAAAATTTE!!!
        }

        private MovementStates movementState = MovementStates.Idle;
        private MovementStates lastMovementState = MovementStates.Idle;

        private MovementStates MovementState
        {
            get { return movementState; }
            set { if (value != movementState) { lastMovementState = movementState; movementState = value; } }
        }

        private int mBacteriaNear = 0; // Internal state. Don't touch
        private int BacteriaNear
        {
            set
            {
                mBacteriaNear = value;
                if (value > 0)
                {
                    MovementState = MovementStates.BaceriaInRange;
                }
                else
                {
                    MovementState = lastMovementState;
                }
            }
            get
            {
                return mBacteriaNear;
            }
        } // Counter for close bacterias. Also sets the state to exterminate!!!

        ModelParameter mParameter;
        private Rigidbody2D mRigidBody;
        private int mBacteriaEaten = 0;

        public float X { get { return transform.position.x; } }
        public float Y { get { return transform.position.y; } }

        public void Start()
        {
            GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
            mParameter = gc.Parameter;
            mRigidBody = GetComponent<Rigidbody2D>();
            if (!mRigidBody)
            {
                Debug.LogError("No rigidBody attached!");
            }
            //clebac = mParameter.PhagocytosysRate; // Phagocytosys rate (min-1, calculated in Excel parameters calculation.xlsx (sheet: Phagocytosys rate) [[PMC266186]])
            StartCoroutine(NewHeadingCoroutine());
        }

        /// <summary>
        /// Set new heading depending on state
        /// </summary>
        private void SetNewHeading()
        {
            Debug.Log(movementState.ToString() + " " + BacteriaNear);
            switch (movementState)
            {
                case MovementStates.Idle:
                    mDirection = new Vector2(Random.Range(-1F, 1F), Random.Range(-1F, 1F)); // Random direction
                    target = this.gameObject;
                    break;
                case MovementStates.ChemokineFound:
                    var cellList = GetObjectsAround<Cell>("Cell", 1.5F);
                    Cell cellWithMaxChemokine = cellList.OrderByDescending(c => c.Chemokine).First();
                    target = cellWithMaxChemokine.gameObject;
                    mDirection = (target.transform.position - transform.position).normalized;
                    break;
                case MovementStates.BaceriaInRange:
                    var bactList = GameObject.FindGameObjectsWithTag("Bacteria").ToList();
                    Bacteria nearestBact = bactList
                        .OrderByDescending(b => Vector2.Distance(transform.position, b.transform.position))
                        .First()
                        .GetComponent<Bacteria>();
                    target = nearestBact.gameObject;
                    mDirection = (target.transform.position - transform.position).normalized;
                    break;
                default:
                    Debug.LogError("Macrophage state not implemented!");
                    break;
            }
        }

        private IEnumerator NewHeadingCoroutine()
        {
            while (true)
            {                
                SetNewHeading();                
                yield return new WaitForSeconds(movementState == MovementStates.Idle ? 3F : 0.1F);
            }
        }
        public Vector2 velocity;
        public void FixedUpdate()
        {
            PlayerMovementClamping();
            var speed = mParameter.MacrophageMovement * 1;
            Debug.DrawLine(transform.position, target.transform.position, Color.black);
            Vector2 myPosition = transform.position; // trick to convert a Vector3 to Vector2
            mRigidBody.MovePosition(myPosition + mDirection * speed * Time.deltaTime);
            // If our spider senses are tingeling and we smell chemokine we switch to search mode.
            // But only if we don't have a bacteria inside. Need to exterminate them first
            if (MovementState != MovementStates.BaceriaInRange)
            {
                var cellList = GetObjectsAround<Cell>("Cell", 1.5F);
                if (cellList.Count > 0 && cellList.Max(c => c.Chemokine) > 0)
                {
                    MovementState = MovementStates.ChemokineFound;
                }
                else
                {
                    MovementState = lastMovementState;
                }
            }
        }

        List<T> GetObjectsAround<T>(string tag, float radius)
        {
            return GameObject.FindGameObjectsWithTag(tag)
                .Where(go => Vector3.Distance(go.transform.position, transform.position) <= radius)
                .Select(go => go.GetComponent<T>())
                .ToList();
        }

        void PlayerMovementClamping()
        {
            var viewpointCoord = Camera.main.WorldToViewportPoint(transform.position);
            viewpointCoord.x = Mathf.Clamp01(viewpointCoord.x);
            viewpointCoord.y = Mathf.Clamp01(viewpointCoord.y);
            transform.position = Camera.main.ViewportToWorldPoint(viewpointCoord);
        }


#region Collider
        /// <summary>
        /// Handles the entry collision with this object
        /// </summary>
        /// <param name="e">other object</param>
        private void OnTriggerEnter2D(Collider2D e)
        {
            if(e.gameObject.name.Contains("Bacteria"))
            {
                var bac = e.gameObject.GetComponent<Bacteria>();
                var distToBact = Vector2.Distance(transform.position, e.transform.position);
                var macBounds = 0.7F;
                if (distToBact < macBounds)
                {
                    BacteriaNear--;
                    Destroy(e.gameObject);
                    mBacteriaEaten++;
                    Debug.Log("Bacteria exterminated");
                    return;
                }
                BacteriaNear++;
            }
        }

        private void OnTriggerStay2D(Collider2D e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                var bac = e.gameObject.GetComponent<Bacteria>();
                var distToBact = Vector2.Distance(transform.position, e.transform.position);
                var macBounds = 0.7F;
                if (distToBact < macBounds)
                {
                    BacteriaNear--;
                    Destroy(e.gameObject);
                    mBacteriaEaten++;
                    Debug.Log("Bacteria exterminated");
                    return;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D e)
        {
            if (e.gameObject.name.Contains("Bacteria"))
            {
                var bac = e.gameObject.GetComponent<Bacteria>();
                BacteriaNear--;
            }
        }
#endregion
    }

}
