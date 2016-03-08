using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private const double[] costArray = {1.20,1.28,1.32,1.42,1.45,1.44,1.54,1.50};
        private const double[] volume = {5000,5000,19000,11000,19000,8000,19000,15000};
        private const double[] expectedAvgCost = {1.2046,0.0,1.3238,1.3606,1.3606,1.3958,0.00,0.0};
        //[TestMethod]
        //public void TestCalculationForGross()
        //{
        //    int i = 0;
        //    while (i < 8)
        //    {
        //        double actualResult = PortfolioController.Fomulas.getGross(costArray[i], volume[i]);
             
        //        Assert.AreEqual(actualResult, );
        //        i++;
        //    }

        //}

        [TestMethod]
        public void TestCalculationForCommission()
        {
            double cost = 100.10;
            double volume = 50;
            double actualResult = PortfolioController.Fomulas.getCommission(cost, volume);

        }

        [TestMethod]
        public void TestCalculationForAvgCost()
        {
            int i = 0;
            while (i < 8)
            {

                i++;
            }
        }

    }
}
