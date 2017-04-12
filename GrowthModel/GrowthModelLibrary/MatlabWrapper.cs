using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Distributions;

namespace GrowthModelLibrary
{
    /// <summary>
    /// This class wraps some Matlab functionalities so the same code can be used
    /// </summary>
    public static class MatlabWrapper
    {
         // Internal static
        private static MatrixBuilder<double> M = Matrix<double>.Build;
        private static VectorBuilder<double> V = Vector<double>.Build;

        public static Matrix<double> rand(int rows, int columns)
        {
            return M.Random(rows, columns);
        }

        public static Matrix<double> zeros(int rows, int columns)
        {
            return M.Dense(rows, columns, 0);
        }

        public static Matrix<double> ones(int rows, int columns)
        {
            return M.Dense(rows, columns, 1);
        }

        public static Matrix<double> random(int mu, int sigma, int rows, int columns)
        {
            return M.Random(rows, columns, new Normal(mu, sigma)); ;
        }

        public static double[] linspace(int start, int stop, int length)
        {
            return Generate.LinearSpaced(length, start, stop);
        }

        public static double[] pdist(Matrix<double> X)
        {
            double[] ret = new double[X.RowCount];
            for(int i = 0; i < ret.Length; i++)
            {
                var x = X[i, 0];
                var y = X[i, 1];
                ret[i] = Math.Sqrt(x*x + y*y);
            }
            return ret;
        }

        public static Matrix<double> squareform(double[] input)
        {
            int mRange = input.Length / 2 + 1;
            Matrix<double> ret = M.Dense(mRange, mRange);
            // iterate rows
            int startingPoint = 1;
            int count = 0;
            for (int r = 0; r < mRange; r++)
            {
                for (int n = startingPoint; n < mRange; n++)
                {                    
                    ret[n, r] = input[count];
                    ret[r, n] = input[count++];                   
                }
                startingPoint++;
            }
            return ret;
        }
    }
}
