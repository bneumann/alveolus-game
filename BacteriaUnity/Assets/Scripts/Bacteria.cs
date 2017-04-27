//using System;
using UnityEngine;

namespace Assets.Scripts
{
	public class Bacteria : MonoBehaviour
	{
		private MovementStates mMovementState = MovementStates.SessileState;
        private ModelParameter mParameter;
        private Vector3 Dimension;

        public float X { get { return transform.position.x; } }
        public float Y { get { return transform.position.y; } }

        public double StepSize
		{
			get
			{
				double step = 0;
				if (mMovementState == MovementStates.FlowingState)
				{
					step = mParameter.MovementInFlowingPhase;
				}
				else
				{
					step = mParameter.MovementInSessilePhase;
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

        public void Start()
        {
            GameController gc = GameObject.Find("GameController").GetComponent<GameController>();
            mParameter = gc.Parameter;
            Dimension = new Vector3(gc.Width, gc.Height);
        }

        private void InterchangePhase()
		{
            var randNum = Random.Range(0, 1);
			// Change state if probability is met. This will change the step size as well
			if (randNum > mParameter.ProbabilityInterchanged)
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

		//public override string ToString()
		//{
		//	return String.Format("Bacteria at {0}|{1} in {2}\n", X, Y, Enum.GetName(typeof(MovementStates), mMovementState));
		//}


		/// <summary>
		/// Moves the bacteria. It will jump to mobile phase with a certain propability
		/// </summary>
		public void Update()
		{
			InterchangePhase();
			InterchangePhase();
			var angl = Random.Range(0F, 1F) * 2 * System.Math.PI; // Random direction
            
			var x = (float)(System.Math.Cos(angl) * StepSize);
			var y = (float)(System.Math.Sin(angl) * StepSize); // Step into the direction defined
            if (x > Dimension.x)
            {
                x = Dimension.x;
            }
            if (x < -Dimension.x)
            {
                x = -Dimension.x;
            }
            if (y > Dimension.y)
            {
                y = Dimension.y;
            }
            if (y < -Dimension.y)
            {
                y = -Dimension.y;
            }
            //Debug.Log("Dimension:" + Dimension +" x " + x + " X " + X);
            //Debug.Log("y " + y + " Y " + Y);
            transform.Translate(new Vector3(x, y, 0));

        }
	}
}
