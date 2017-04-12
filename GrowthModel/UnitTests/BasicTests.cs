using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static GrowthModelLibrary.MatlabWrapper;
using MathNet.Numerics.LinearAlgebra;

namespace UnitTests
{
    [TestClass]
    public class MatlabWrapperTests
    {
        [TestMethod]
        public void TestZeros()
        {
            var z = zeros(4, 4);
            Assert.IsTrue(z.ForAll(p => p == 0));
        }

        [TestMethod]
        public void TestOnes()
        {
            var z = ones(4, 4);
            Assert.IsTrue(z.ForAll(p => p == 1));
        }

        [TestMethod]
        public void TestPDist()
        {
            double[,] x = {{ 1.0, 2.0, 3.0 },
                           { 3.0, 4.0, 5.0 }};
            Matrix<double> m = Matrix<double>.Build.DenseOfArray(x).Transpose();

            double[] z = pdist(m);
            double[] res = { Math.Sqrt(10), Math.Sqrt(20), Math.Sqrt(34) };

            // Test with double arrays is failing because of rounding issues
            Assert.AreEqual(Vector<double>.Build.DenseOfArray(res), Vector<double>.Build.DenseOfArray(z));
        }

        [TestMethod]
        public void TestSquareform()
        {
            var z = new double[] { 1, 2, 3, 4, 5, 6 };
            double[,] X = {{ 0, 1, 2, 3 },
                           { 1, 0, 4, 5 },
                           { 2, 4, 0, 6 },
                           { 3, 5, 6, 0 }};
            Matrix<double> check = Matrix<double>.Build.DenseOfArray(X);                              
            Matrix<double> res = squareform(z);
            Assert.AreEqual(res, check);
        }
    }
}
