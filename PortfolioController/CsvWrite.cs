using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace PortfolioController
{
    public class CsvWrite
    {
        private static object syncRoot = new Object();


        public static void WritePledge(List<Client> list)
        {
            var d = DateTime.Now.ToString("M-dd-yyyy");
            string path1 = String.Format(ConfigurationManager.AppSettings["txtFilesBackupPath"].ToString() + "pledge-{0}.txt", d);
            string path2 = ConfigurationManager.AppSettings["txtFilesPath"].ToString() + "pledge.txt";
            string delimiter = ",";
            StringBuilder sb = new StringBuilder();
            //OT9521-7;MARCO N. ACCAD;Unit 406 Danube Bldg., Riverfront Residences Caniogan, Pasig 1606 ;(0917)862-8504 631-8734;0.0025;N;C;5;11011205464539

            foreach (var m in list)
            {
                sb.Append(m.AccountCode);
                sb.Append(delimiter);
                sb.Append("0.00\r\n");
            }
            sb.AppendLine("");
            sb.AppendLine("");
            AppendToFile(System.Environment.ExpandEnvironmentVariables(path1), sb.ToString());
            AppendToFile(System.Environment.ExpandEnvironmentVariables(path2), sb.ToString());

        }

        public static void WriteClient(List<Client> list)
        {
            var d = DateTime.Now.ToString("M-dd-yyyy");
            string path1 = String.Format(ConfigurationManager.AppSettings["txtFilesBackupPath"].ToString() + "clients-{0}.txt", d);
            string path2 = ConfigurationManager.AppSettings["txtFilesPath"].ToString() + "clients.txt";
            string delimiter = ";";
            StringBuilder sb = new StringBuilder();
            //OT9521-7;MARCO N. ACCAD;Unit 406 Danube Bldg., Riverfront Residences Caniogan, Pasig 1606 ;(0917)862-8504 631-8734;0.0025;N;C;5;11011205464539

            foreach (var m in list)
            {
                sb.Append(m.AccountCode);
                sb.Append(delimiter);
                sb.Append(m.Name);
                sb.Append(delimiter);

                string x = "1011119999";
                int five = m.ClientId;
                x = x + five.ToString().PadLeft(4, '0');

                sb.Append(String.Format("Makati;(0917)862-8504 631-8734;0.0025;N;C;5;{0}\r\n",x));
            }
            sb.AppendLine("");
            sb.AppendLine("");
            AppendToFile(System.Environment.ExpandEnvironmentVariables(path1), sb.ToString());
            AppendToFile(System.Environment.ExpandEnvironmentVariables(path2), sb.ToString());

        }


        public static void write(MatchedOrder[] list)
        {
            var d = DateTime.Now.ToString("ddMMyyy");
            string path = String.Format(ConfigurationManager.AppSettings["matchedOrdersCsvPath"].ToString()+"{0}-matchedorders.csv",d);
            string delimiter = ",";
            StringBuilder sb = new StringBuilder();

            foreach (MatchedOrder m in list)
            {
                sb.Append(m.UserId + delimiter);
                sb.Append(m.AccountCode + delimiter);
                sb.Append(m.OrderDatetime + delimiter);
                sb.Append(m.Side + delimiter);
                sb.AppendLine(m.BoardLot);
                sb.Append(m.StockCode + delimiter);
                sb.Append(m.Price + delimiter); 
                sb.Append(m.Quantity + delimiter);
               
            }

            AppendToFile(System.Environment.ExpandEnvironmentVariables(path), sb.ToString());
            
        }

        public static string data(Dealers dealers)
        {           
            string delimiter = ",";


            StringBuilder sb = new StringBuilder();

            Console.WriteLine("PORTFOLIO\tDATE\t\tSECURITY_ID\tPOSITION\tAVG_COST");

            sb.Append("PORTFOLIO" + delimiter);
            sb.Append("DATE" + delimiter);
            sb.Append("SECURITY_ID" + delimiter);
            sb.Append("POSITION" + delimiter);
            sb.Append("AVG_COST");
            sb.AppendLine("");

            foreach (PortfolioModel p in dealers.D1)
            {
                WriteToConsole(p);
                sb.Append(p.PortfolioName + delimiter);
                sb.Append(p.Date + delimiter);
                sb.Append(p.SecurityID + delimiter);
                sb.Append(p.Position + delimiter);
                sb.Append(p.AvgCost);
                sb.AppendLine("");
            }

            foreach (PortfolioModel p in dealers.Dasys)
            {
                WriteToConsole(p);
                sb.Append(p.PortfolioName + delimiter);
                sb.Append(p.Date + delimiter);
                sb.Append(p.SecurityID + delimiter);
                sb.Append(p.Position + delimiter);
                sb.Append(p.AvgCost);
                sb.AppendLine("");
            }

            foreach (PortfolioModel p in dealers.D4)
            {
                WriteToConsole(p);
                sb.Append(p.PortfolioName + delimiter);
                sb.Append(p.Date + delimiter);
                sb.Append(p.SecurityID + delimiter);
                sb.Append(p.Position + delimiter);
                sb.Append(p.AvgCost);
                sb.AppendLine("");
            }

            foreach (PortfolioModel p in dealers.Mm)
            {
                WriteToConsole(p);
                sb.Append(p.PortfolioName + delimiter);
                sb.Append(p.Date + delimiter);
                sb.Append(p.SecurityID + delimiter);
                sb.Append(p.Position + delimiter);
                sb.Append(p.AvgCost);
                sb.AppendLine("");
            }
            
            string toCsv = sb.ToString();          
            return toCsv;
        }

        private static void WriteToConsole(PortfolioModel p)
        {
       
            Console.Write(p.PortfolioName + "\t\t" + p.Date);
            if (!p.PortfolioName.Equals("DASYS"))
            {

                if (p.SecurityID.Equals("php Curncy"))
                {
                    Console.Write("\t" + p.SecurityID + "\t" + p.Position + "\t\t" + p.AvgCost);
                }
                else
                {
                    Console.Write("\t" + p.SecurityID + "\t\t" + p.Position + "\t\t" + p.AvgCost);
                }
            }
            else
            {
                if (p.SecurityID.Equals("php Curncy"))
                {
                    Console.Write("\t" + p.SecurityID + "\t" + p.Position + "\t\t" + p.AvgCost);
                }
                else
                {
                    Console.Write("\t" + p.SecurityID + "\t\t" + p.Position + "\t\t" + p.AvgCost);
                }
            }
            Console.WriteLine("");

        }
        public void AppendToFile(String lineEntry)
        {
            string logFilePath = ConfigurationManager.AppSettings["logFilePath"].ToString();
            AppendToFile(logFilePath, lineEntry);
        }
        private static void AppendToFile(String file, String lineEntry)
        {
            lock (syncRoot)
            {

                string path = Environment.ExpandEnvironmentVariables(file);

                if (!File.Exists(path))
                {
                    using (File.Create(path)) { };
                }
                using (FileStream f = new FileStream(path, FileMode.Create))
                {
                    using (StreamWriter s = new StreamWriter(f))
                    {
                        s.WriteLine(lineEntry);

                    }
                }
            }


        }
    }

    //public class Calculator
    //{
    //    public static List<model.Portfolio> calculateCurrencyPosCost(List<model.Portfolio> p)
    //    {
    //        double d1 = 0;
    //        double dasys = 0;
    //        double d4 = 0;
    //        double mm = 0;
    //        double er = 0;

    //        double d1Capital = 0;
    //        double.TryParse(ConfigurationManager.AppSettings["dealer1capital"].ToString(), out d1Capital);

    //        double dasysCapital = 0;
    //        double.TryParse(ConfigurationManager.AppSettings["dasyscapital"].ToString(), out dasysCapital);
            
    //        double d4Capital = 0;
    //        double.TryParse(ConfigurationManager.AppSettings["dealer4capital"].ToString(), out d4Capital);
            
    //        double mmCapital = 0;
    //        double.TryParse(ConfigurationManager.AppSettings["mmcapital"].ToString(), out mmCapital);


    //        foreach (model.Portfolio a in p)
    //        { 
    //            if (a.PortfolioName == "D1") d1 += pcost;
    //            if (a.PortfolioName == "DASYS") dasys += pcost;
    //            if (a.PortfolioName == "D4") d4 += pcost;
    //            if (a.PortfolioName == "MM") mm += pcost;
    //            if (a.PortfolioName == "ER") er += pcost;
    //        }

    //        foreach (model.Portfolio a in p)
    //        {
    //            if (a.PortfolioName == "D1" && a.SecurityID == "php Curncy")
    //            {
    //                a.Poscost = ((d1Capital) - d1).ToString();
    //            }
    //            else if (a.PortfolioName == "DASYS" && a.SecurityID == "php Curncy")
    //            {
    //                a.Poscost = ((dasysCapital) - dasys).ToString();
    //            } 
    //            else if (a.PortfolioName == "D4" && a.SecurityID == "php Curncy")
    //            {
    //                a.Poscost = ((d4Capital) - d4).ToString();
    //            } 
    //            else if (a.PortfolioName == "MM" && a.SecurityID == "php Curncy")
    //            {
    //                a.Poscost = ((mmCapital) - mm).ToString();
    //            }
    //        }

    //        return p;
    //    }
    }

