using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;
using static GrowthModelLibrary.MatlabWrapper;
using System.Threading;

namespace GrowthModelLibrary
{

	public class Game
	{
		// Defaults and statics
		int output = 0;     // initial number of bacteria spread
		int n = 0;          // Iterations
		int nb;             // Initial number of bacteria
		int Nc = 11;        // Number of cells per row in alveolus
		int cel = 30;       // Width of epitheliaol cell in microm
		int nummac;         // Number of macrophages, approximation from Bionumbers
		int nstop = 0;      // Counting stoping time between the models
		int foco = 4;       // Dispersion of initial bacteria in the alveolus
							//int writerObj1;     // Create a movie
							//int movie;
							//int final;
							//int pars;
		Matrix<double> step; // Movement rate in the sesile phase(step1)
		double step1;       // Movement rate in the sesile phase, assumed
		double step2;       // Movemenet rate in the mobile phase, assumed
		double step3;       // min-1. Radial flow(4.2e-2) Fitted to alveolar clearance in figura 3B[[PMID: 17290033]] calculated in Excel parameters calculation.xlsx(sheet: alveoli flow) 
							//int pantalla;

		// Generated properties
		Matrix<double> neat;        // Intrinsic macrophage time for bacteria phagocytosis
		Matrix<double> eaten;       // Register of eated bacteria
		int dimens;                 // Dimension of the alveoli in micrometers from Bionumbers (11 from 121 cells)
		Matrix<double> position;    // Normal distribution of initial bacteria
		Matrix<double> mac;         // Random localization of macrophages
									// FIXME: Is actually a list that will be increased over time:
		Matrix<double> Chemokine;   // Chemokine 

		//Biological parameters for bacteria
		double growth;                  // min-1. 199.2 min doubling time (3.32 hours from [[pag 38 book: ISBN 978-1-908230-17-1]])Iterations per division (5e2)
		double probS;                   // Probability of not being interchanged between phases (0.999) A lower value stablish the infection with low probability 
		double probC = 0.995;           // Probability of not leave the alveolus from the mobile phase
		double maxbac = 1e3;            // Maximum number of bacteria in the alveolus simulated, not relevant
		double rad = 1.5;               // Region for determine the saturation of bacteria (1.5) Assuming two hexagonal layers
		double Mbac;                    // Maximum number of bacteria inside the region to saturate the growth (10) Assuming two hexagonal layers


		public List<GameObject> worldObjects = new List<GameObject>();
		private ModelParameter mParameter;
		public CollisionDetector collisionDetector;
		private int mIteration = 0;

		public int Width { get { return mParameter.CellDimension; } }
		public int Height { get { return mParameter.CellDimension; } }

		public Game()
		{
			mParameter = new ModelParameter() { /*EpithelialCellsPerRow = 40, BacteriaDoublingTime = 1*/};

			worldObjects.Add(new Cell(mParameter));

			for (int i = 0; i < mParameter.NumberOfMacrophages; i++)
			{
				Macrophage m = new Macrophage(mParameter);
				worldObjects.Add(m);
			}

			for (int nb = 0; nb < mParameter.NumberOfBacteria; nb++)
			{
				worldObjects.Add(new Bacteria(mParameter));
			}
			collisionDetector = new CollisionDetector(worldObjects);
		}

		public void Update()
		{
			lock (worldObjects)
			{
				if (mIteration % mParameter.BacteriaDoublingTime == 0)
				{
					var bactList = worldObjects.Where(wo => wo.GetType() == typeof(Bacteria)).ToArray();
					for (int i = 0; i < bactList.Length; i++)
					{
						var b = bactList[i];
						worldObjects.Add(new Bacteria(mParameter) { X = b.X, Y = b.Y });
					}

				}
				worldObjects.ToList().ForEach(go => go.Update());
				collisionDetector.Update();
				mIteration++;
			}
		}

		public void MainThread()
		{
			while (true)
			{
				Update();
				Thread.Sleep(1);
			}
		}

	}

	// TODO: don't extend List but compose it from worldList 
	public class Infection : List<Bacteria>
	{
		private int mIteration = 0;
		private int mGrowthRate = 0; // See Modelparameter BacteriaDoublingTime
		private double mSaturationRadius = 1.5; // Region for determine the saturation of bacteria (1.5) Assuming two hexagonal layers
		private int mBacteriaSaturation; //See Modelparameter BacteriaSaturationNumber
		private ModelParameter mParameter;
		private static Random mRandom = new Random();

		public Infection(ModelParameter parameter)
		{
			mParameter = parameter;
			mBacteriaSaturation = 10 * mParameter.BacteriaSaturationNumber;
			mGrowthRate = (int)Math.Round((double)(200 * mParameter.BacteriaDoublingTime));

			for (int nb = 0; nb < parameter.NumberOfBacteria; nb++)
			{
				this.Add(new Bacteria(parameter));
			}
		}

		public void Cough()
		{
			var probC = 0.995;
			var rand = mRandom.NextDouble();
			foreach (Bacteria b in this)
			{
				if (mRandom.NextDouble() > probC && b.State == Bacteria.MovementStates.FlowingState)
				{
					this.Remove(b);
				}
			}
		}

		public void Grow()
		{
			if (mIteration % mGrowthRate == 0)
			{
				// Not needed according to Guido
				//var collection = new HashSet<Bacteria>();
				//foreach(Bacteria ba in this)
				//{
				//    var tmpCollection = new HashSet<Bacteria>();
				//    foreach (Bacteria bb in this)
				//    {
				//        // add all bacteria that are in range and not the bacteria itself
				//        if(ba.GetDistance(bb) < mSaturationRadius) // !ba.Equals(bb) 
				//        {
				//            tmpCollection.Add(ba);
				//        }
				//    }
				//    // If we don't have too many neighbours we can grow
				//    if (tmpCollection.Count < mBacteriaSaturation)
				//    {
				//        tmpCollection.ToList().ForEach(b => collection.Add(b));
				//    }
				//}
				foreach (Bacteria b in this.ToList())
				{
					this.Add(new Bacteria(mParameter) { X = b.X, Y = b.Y });
				}
				if (this.Count() == 1)
				{
					this.Add(new Bacteria(mParameter) { X = this.First().X, Y = this.First().Y });
				}
			}
			this.ForEach(b => b.Update());
			mIteration++;
		}
	}



}
