using System;
using UnityEngine;

namespace Assets.Scripts
{
	public class Macrophage : MonoBehaviour
	{
		private int mEatenBacteria = 0;
		// Biological parameters for macrophages
		int macsiz = 10; // Macrophage size in micras (21 microm of diameter, from Wikipedia)
		int stepm;
		double clebac;
		double sensit;

        ModelParameter mParameter;
        static System.Random mRandom = new System.Random();

        public float X { get { return transform.position.x; } }
        public float Y { get { return transform.position.y; } }

        public Macrophage(ModelParameter parameter)
		{
            mParameter = parameter;
            clebac = parameter.PhagocytosysRate; // Phagocytosys rate (min-1, calculated in Excel parameters calculation.xlsx (sheet: Phagocytosys rate) [[PMC266186]])
			sensit = parameter.SensitivityToFeelCytokineGradient; // Sensitivity to feel the cytokine gradient [[buscar]].
		}
		public void Update()
		{
			var angl = mRandom.NextDouble() * 2 * Math.PI; // Random direction
			var x = X + (int)(Math.Cos(angl) * mParameter.MacrophageMovement);
			var y = Y + (int)(Math.Sin(angl) * mParameter.MacrophageMovement); // Step into the direction defined
            transform.Translate(new Vector3(x, y, 0));
		}

		//public override void Collision(GameObject otherObject)
		//{
		//	if (otherObject.GetType() == typeof(Bacteria))
		//	{
		//		mEatenBacteria++;
		//		otherObject.isDead = true;
		//		Console.WriteLine("Bacteria eaten: {0}", mEatenBacteria);
		//		if (mEatenBacteria >= mParameter.MaximumBacteriaPerMacrophage)
		//		{
		//			this.isDead = true;
		//		}
		//	}
		//}
	}

}
