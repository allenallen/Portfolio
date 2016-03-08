using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio
{
    public class Fomulas
    {
        public static double getGross(double cost, double volume)
        {
            if (volume == 0) return 0;
            return cost * volume;
        }
        public static double getCommission(double cost, double volume)
        {
            if (volume == 0) return 0;
            if ((cost * volume * 0.0025) < 20) { return 20; }
            else { return cost * volume * 0.0025; }
        }
        public static double getVat12(double commission)
        {
            return commission * .12;
        }
        public static double getSalesTax(double gross)
        {
            return gross * .005;
        }
        public static double getPCDSCCP(double gross)
        {
            return gross * .0001;
        }
        public static double getBuyPrice(double gross, double commission, double vat12, double PCDSCCP)
        {
            return gross + commission + vat12 + PCDSCCP;
        }
        public static double getSellPrice(double gross, double salesTax,double commission, double vat12, double PCDSCCP)
        {
            return gross - (commission + vat12 + salesTax + PCDSCCP);
        }
        public static double getNetPrice(string side, double buyPrice, double sellPrice)
        {
            if (side.Equals("buy"))
            {
                return buyPrice;
            }
            else if (side.Equals("sell"))
            {
                return sellPrice;
            }
            else
            {
                return 0.01;
            }
        }
        public static double getSumOfNetPrice(bool isFirstTransaction,string side,double volume,double netVolume,double currentVolume,double currentNetPrice, double lastAverageCost,double lastSumOfNetPrice)
        {
            if(isFirstTransaction)
            {
                if(side.Equals("sell"))
                {
                    return 0;
                }
                else
                {
                    return currentNetPrice;
                }
            }
            else
            {
                if(side.Equals("sell"))
                {
                    return netVolume * lastAverageCost;
                }
                else
                {
                    return lastSumOfNetPrice + currentNetPrice;
                }
            }
        }
        public static double netVolume(bool isFirstTransaction, string side, double volume,double lastNetVolume, double currentVolume)
        {
            if (isFirstTransaction)
            {
                if (side.Equals("sell"))
                {
                    return 0;
                }
                else
                {
                    return volume;
                }
            }
            else
            {
                if (side.Equals("sell")) currentVolume = -1 * currentVolume;
                return lastNetVolume + currentVolume;
            }
        }
        public static double getAverageCost(double netVolume, double sumOfNetPrice)
        {
            if (netVolume == 0)
            {
                return 0;
            }
            else
            {
                return sumOfNetPrice / netVolume;
            }
        }
    }
}
