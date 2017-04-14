﻿using System;
namespace GrowthModelLibrary
{
	public class Bacteria : GameObject
	{
		private MovementStates mMovementState = MovementStates.SessileState;

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

		public override double Radius
		{
			get
			{
				return 0;
			}
		}

		public enum MovementStates
		{
			SessileState,
			FlowingState
		}

		public Bacteria(ModelParameter parameter) : base(parameter)
		{

		}

		private void InterchangePhase()
		{
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


		/// <summary>
		/// Moves the bacteria. It will jump to mobile phase with a certain propability
		/// </summary>
		public override void Update()
		{
			InterchangePhase();
			InterchangePhase();
			var angl = mRandom.NextDouble() * 2 * Math.PI; // Random direction
			X += (int)(Math.Cos(angl) * StepSize);
			Y += (int)(Math.Sin(angl) * StepSize); // Step into the direction defined
		}

		public override void Collision(GameObject otherObject)
		{
		}
	}
}
