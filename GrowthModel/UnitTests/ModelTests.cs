using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra;
using GrowthModelLibrary;

namespace UnitTests
{
    [TestClass]
    public class ModelTests
    {

        Bacteria bact;

        [TestInitialize]
        public void TestInit()
        {
            bact = new Bacteria(new ModelParameter());
        }

        [TestMethod]
        public void TestBacteriaDistance()
        {
            bact = new Bacteria(new ModelParameter());
            var b2 = new Bacteria(new ModelParameter());
            b2.X = 3;
            b2.Y = 4;
            bact.X = 0;
            bact.Y = 0;
            Assert.AreEqual(bact.GetDistance(b2), 5.0);
        }

        [TestMethod]
        public void TestBacteriaMovement()
        {
            bact = new Bacteria(new ModelParameter());
            int oldX = bact.X;
            bact.Move();
            Assert.AreNotEqual(oldX, bact.X);
        }

        [TestMethod]
        public void TestInfectionGrowth()
        {
            Infection i = new Infection(new ModelParameter());
            i.Grow();
        }
    }
}
