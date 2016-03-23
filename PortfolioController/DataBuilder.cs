using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class DataBuilder
    {
        public static List<PosCost> GetPosCostTableData()
        {
            using(var dbContext = new StockXDataBaseEntities())
            {
                var posCostTable = dbContext.PosCost.ToList();
                return posCostTable;
            }
        }

        public static List<Summary> GetSummaryTableDate()
        {
            using (var dbContext = new StockXDataBaseEntities())
            {
                var summaryTable = dbContext.Summary.ToList();
                return summaryTable;
            }
        }

        public static List<MatchedOrder> GetMatchedOrderTableData()
        {
            using (var dbContext = new StockXDataBaseEntities())
            {
                var matchedOrderTable = dbContext.MatchedOrder.ToList();
                return matchedOrderTable;
            }
        }

        public static Dealers ListCurrentPosCost(IEnumerable<string> csvLine)
        {
            Dealers dealers = new Dealers();
            List<PortfolioModel> d1 = new List<PortfolioModel>();
            List<PortfolioModel> dasys = new List<PortfolioModel>();
            List<PortfolioModel> d4 = new List<PortfolioModel>();
            List<PortfolioModel> mm = new List<PortfolioModel>();
            Dictionary<string, double> currency = new Dictionary<string, double>();
            foreach(string s in csvLine)
            {
                if (s.Equals("")) continue;
                string[] data = s.Split(";".ToCharArray());
              
                PortfolioModel d = new PortfolioModel();
                d.PortfolioName = data[0];
                d.Date = GetDate();
                d. SecurityID =    String.Format("{0} PM EQUITY", data[1].ToUpper());
                d.Position = data[2];
                d.AvgCost = data[3];

                switch(d.PortfolioName)
                {
                    case "DEALER01":
                        d.PortfolioName = ChangeName(d.PortfolioName);
                        d1.Add(d);
                        break;
                    case "DASYS001":
                        d.PortfolioName = ChangeName(d.PortfolioName);
                        dasys.Add(d);
                        break;
                    case "DEALER04":
                        d.PortfolioName = ChangeName(d.PortfolioName);
                        d4.Add(d);
                        break;
                    case "FMETF-MM":
                        d.PortfolioName = ChangeName(d.PortfolioName);
                        mm.Add(d);
                        break;
                }           
            }
            dealers.D1 = d1;
            dealers.D4 = d4;
            dealers.Dasys = dasys;
            dealers.Mm = mm;
            CalculateCurrency(currency, dealers);
            dealers.Currency = currency;
            AddCurrencyRow(dealers);
            return dealers;
        }

        private static void AddCurrencyRow(Dealers dealer)
        {
            string[] dealers = { "D1", "D4", "DASYS", "MM", "ER"};
            foreach (string s in dealers)
            {
                PortfolioModel pm = new PortfolioModel();
                switch (s)
                {
                    case "D1":
                        pm.PortfolioName = s;
                        pm.Date = GetDate();
                        pm.SecurityID = "php Curncy";
                        pm.Position = "1";
                        double a;
                        dealer.Currency.TryGetValue(s,out a);
                        pm.AvgCost = a.ToString();
                        dealer.D1.Add(pm);
                        break;
                    case "DASYS":
                        pm.PortfolioName = s;
                        pm.Date = GetDate();
                        pm.SecurityID = "php Curncy";
                        pm.Position = "1";
                        double b;
                        dealer.Currency.TryGetValue(s, out b);
                        pm.AvgCost = b.ToString();
                        dealer.Dasys.Reverse();
                        dealer.Dasys.Add(pm);
                        dealer.Dasys.Reverse();                        
                        break;
                    case "D4":
                        pm.PortfolioName = s;
                        pm.Date = GetDate();
                        pm.SecurityID = "php Curncy";
                        pm.Position = "1";
                        double c;
                        dealer.Currency.TryGetValue(s, out c);
                        pm.AvgCost = c.ToString();
                        dealer.D4.Add(pm);
                        break;
                    case "MM":
                        pm.PortfolioName = s;
                        pm.Date = GetDate();
                        pm.SecurityID = "php Curncy";
                        pm.Position = "1";
                        double d;
                        dealer.Currency.TryGetValue(s, out d);
                        pm.AvgCost = d.ToString();
                        dealer.Mm.Add(pm);
                        break;
                    case "ER":
                        pm.PortfolioName = s;
                        pm.Date = GetDate();
                        pm.SecurityID = "php Curncy";
                        pm.Position = "0";                       
                        pm.AvgCost = "0";
                        dealer.Mm.Add(pm);
                        break;
                }

            }
        }

        private static string GetDate()
        {

            if (DateTime.Now.ToString("ddd").Equals("Mon"))
            {
                return DateTime.Now.AddDays(-3).ToString("MM/dd/yyy");
            }
            else
            {
                DateTime cal = DateTime.Now.AddDays(-1);
                return cal.ToString("MM/dd/yyy");
            }
        }

        private static void CalculateCurrency(Dictionary<string,double> crncy,Dealers dealer)
        {
            double d1Capital;
            double.TryParse(ConfigurationManager.AppSettings["dealer1capital"].ToString(), out d1Capital);
            double dasysCapital;
            double.TryParse(ConfigurationManager.AppSettings["dasyscapital"].ToString(), out dasysCapital);
            double d4Capital;
            double.TryParse(ConfigurationManager.AppSettings["dealer4capital"].ToString(), out d4Capital);
            double mmCapital;
            double.TryParse(ConfigurationManager.AppSettings["mmcapital"].ToString(), out mmCapital);

            double d1C = 0;
            double mmC = 0;
            double daC = 0;
            double d4C = 0;

            if (dealer.D1.Count == 0)
            {
                d1C = 0;
                crncy.Add("D1", (d1Capital - d1C));
            }
            else
            {
                foreach (PortfolioModel p in dealer.D1)
                {
                    d1C += Formulas.getPosCost(Convert.ToDouble(p.Position), Convert.ToDouble(p.AvgCost));
                }
                crncy.Add(dealer.D1[0].PortfolioName, (d1Capital - d1C));
            }

            if (dealer.Dasys.Count == 0)
            {
                daC = 0;
                crncy.Add("DASYS", (dasysCapital - daC));
            }
            else
            {
                foreach (PortfolioModel p in dealer.Dasys)
                {
                    daC += Formulas.getPosCost(Convert.ToDouble(p.Position), Convert.ToDouble(p.AvgCost));
                }
                crncy.Add(dealer.Dasys[0].PortfolioName, (dasysCapital - daC));
            }


            if (dealer.D4.Count == 0)
            {
                d4C = 0;
                crncy.Add("D4", (d4Capital - d4C));
            }
            else
            {
                foreach (PortfolioModel p in dealer.D4)
                {
                    d4C += Formulas.getPosCost(Convert.ToDouble(p.Position), Convert.ToDouble(p.AvgCost));
                }
                crncy.Add(dealer.D4[0].PortfolioName, (d4Capital - d4C));
            }

            if (dealer.Mm.Count == 0)
            {
                mmC = 0;
                crncy.Add("FMETF", (mmCapital - mmC));
            }
            else
            {
                foreach (PortfolioModel p in dealer.Mm)
                {
                    mmC += Formulas.getPosCost(Convert.ToDouble(p.Position), Convert.ToDouble(p.AvgCost));
                }
                crncy.Add(dealer.Mm[0].PortfolioName, (mmCapital - mmC));
            }
           
            
            
            
            
        }

        private static string ChangeName(string name)
        {
            switch (name)
            {
                case "DEALER01":
                    name = "D1";
                    break;
                case "DASYS001":
                    name = "DASYS";
                    break;
                case "DEALER04":
                    name = "D4";
                    break;
                case "FMETF-MM":
                    name = "MM";
                    break;
            }
            return name;
        }

    }
}
