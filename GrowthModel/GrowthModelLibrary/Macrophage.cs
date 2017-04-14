using System;
using MathNet.Numerics.LinearAlgebra;

namespace GrowthModelLibrary
{
	public class Macrophage : GameObject
	{
		// Biological parameters for macrophages
		int macsiz = 10; // Macrophage size in micras (21 microm of diameter, from Wikipedia)
		int stepm;
		double clebac;
		int maxeat;
		double sensit;
		Matrix<double> indMac;

		public override double Radius
		{
			get
			{
				return 10;
			}
		}

		public Macrophage(ModelParameter parameter) : base(parameter)
		{
			stepm = 2 * parameter.MacrophageMovement; // Movement of macrophages (2 micrm/min de [[PMID: 26202827]])
			clebac = 0.048 * parameter.PhagocytosysRate; // Phagocytosys rate (min-1, calculated in Excel parameters calculation.xlsx (sheet: Phagocytosys rate) [[PMC266186]])
			maxeat = 50 * parameter.MaximumBacteriaPerMacrophage; // Maximum number of bacteria attached [[buscar]] // // //  More than 70//  of the macrophage perimeter covered of bacteria inside one minute
			sensit = 1e-6 * parameter.SensitivityToFeelCytokineGradient; // Sensitivity to feel the cytokine gradient [[buscar]].
		}
		public override void Update()
		{
			throw new NotImplementedException();
		}

		public override void Collision(GameObject otherObject)
		{
			if (otherObject.GetType() == typeof(Bacteria))
			{
				otherObject.isDead = true;
			}
		}
	}

}
