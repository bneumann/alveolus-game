using UnityEngine;

namespace Assets.Scripts
{
	/// <summary> 
	/// Parameter for the Growth Model
	/// 1.number of bacteria (1),
	/// 2.number of macrophages (3), 
	/// 3.chemokine diffusion constant (6e3),
	/// 4.antigens per bacteria (1),
	/// 5.Movement in sessile phase (0.1),
	/// 6.Movement in flowing phase (3),
	/// 7.Radial flow (4.2e-2),
	/// 8.Bacteria dobling time (200),
	/// 9.Probability interchanged (0.999),
	/// 10.Bacteria saturation number (10),
	/// 11.Macrophage movement (2),
	/// 12.Phagocytosys rate (0.048),
	/// 13.Maximum bacteria per macrophage (50)
	/// 14.Distance to sense the metabolic gradient (30)
	/// 15.Sensitivity to feel the cytokine gradient (1e-6)]
	/// </summary>
	public class ModelParameter
	{
		public int NumberOfBacteria = 1;
		public int NumberOfMacrophages = 3;
		public int ChemokineDiffusionConstant = 6000;
		public int AntigenPerBacteria = 1;
		public double MovementInSessilePhase = 0.1f;
		public double MovementInFlowingPhase = 3f;
		public double RadialFlow = 4.2e-2;
		public int BacteriaDoublingTime = 200; // every 3.32 hours
		public double ProbabilityInterchanged = 0.999;
		public int BacteriaSaturationNumber = 10;
		public int MacrophageMovement = 2;
		public double PhagocytosysRate = 0.048;
		public int MaximumBacteriaPerMacrophage = 50;
		public int DistanceToSenseMetabolicGradient = 30;
		public double SensitivityToFeelCytokineGradient = 1e-6;
		public int EpithelialCellsPerRow = 11;
		public int EpithelialCellWidth = 30;
		public int CellDimension { get { return (EpithelialCellWidth * EpithelialCellsPerRow) / 2; } }
	
	}
}

