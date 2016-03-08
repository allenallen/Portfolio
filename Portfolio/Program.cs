using FileHelpers;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using PortfolioController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio
{ 
    class Program
    {
        private static object syncRoot = new Object();
        static void Main(string[] args)
        {
            var s = new PortfolioController.StockXDBController();
           
        }

        static void Main2(string[] args)
        {
            string currentPosCostFile = GetFilePath(ConfigurationManager.AppSettings["rootPathToPoscost"].ToString(), 
                ConfigurationManager.AppSettings["poscostFileName"].ToString());

            string previousPosCostFile = GetFilePath(ConfigurationManager.AppSettings["rootPathToPreviousPoscost"].ToString(),
                ConfigurationManager.AppSettings["previousPoscostFileName"].ToString());
            
            Console.WriteLine("poscost.txt filepath: " + currentPosCostFile);
            Console.WriteLine("poscost.txt copy to: " + previousPosCostFile);
            
            try
            {
                if (!File.Exists(currentPosCostFile)) throw new Exception("File Not Found");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }

            var previousPosCost = System.IO.File.ReadAllLines(previousPosCostFile).Skip(1);
            Dealers previousDealers = DataBuilder.ListCurrentPosCost(previousPosCost);
            var currentPoscost = System.IO.File.ReadAllLines(currentPosCostFile).Skip(1);
            Dealers currentDealers = DataBuilder.ListCurrentPosCost(currentPoscost);
            
            string currentPosCostCopy = String.Join(Environment.NewLine, currentPoscost);
            AppendToFile(previousPosCostFile, currentPosCostCopy);

            Dealers dealersForBloomberg = CheckPoscostDifference(previousDealers, currentDealers);

            string pathToActualFile = Environment.ExpandEnvironmentVariables(ConfigurationManager.AppSettings["filepath"].ToString());
            string actualFileName = String.Format("{0}_{1}.csv", ConfigurationManager.AppSettings["actualfilename"].ToString(), DateTime.Now.ToString("MM-dd-yyy"));
            pathToActualFile = System.IO.Path.Combine(pathToActualFile, actualFileName);
            Console.WriteLine("file for bloomberg saved at: " + pathToActualFile);
            Console.WriteLine("");

            string poscos = CsvWrite.data(dealersForBloomberg);
            AppendToFile(pathToActualFile, poscos);

            Console.WriteLine("");
            string uploadToWINSCP = ConfigurationManager.AppSettings["uploadToWinscp"].ToString();
            if (uploadToWINSCP.Equals("false"))
            {
                Console.WriteLine("upload to winscp disabled. press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }
            Console.WriteLine("Uploading to WinSCP..");

            WinSCP.uploadFile(pathToActualFile);
            string disableUserExit = ConfigurationManager.AppSettings["disableUserExit"].ToString();
            if (disableUserExit.Equals("false"))
            {
                Console.WriteLine("done. press any key to exit");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("done.");
            }
             
        }

        private static string GetFilePath(string p1, string p2)
        {
            string rootPath = Environment.ExpandEnvironmentVariables(p1);

            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            rootPath = System.IO.Path.Combine(rootPath, p2);
            return rootPath;
        }

        private static Dealers CheckPoscostDifference(Dealers previousDealers, Dealers currentDealers)
        {
            List<PortfolioModel> prevD1 = previousDealers.D1;
            List<PortfolioModel> prevDasys = previousDealers.Dasys;
            List<PortfolioModel> prevD4 = previousDealers.D4;
            List<PortfolioModel> prevMm = previousDealers.Mm;

            List<PortfolioModel> currD1 = currentDealers.D1;
            List<PortfolioModel> currDasys = currentDealers.Dasys;
            List<PortfolioModel> currD4 = currentDealers.D4;
            List<PortfolioModel> currMm = currentDealers.Mm;

            List<PortfolioModel> d1Difference = checkDifference(prevD1, currD1);
            List<PortfolioModel> dasysDifference = checkDifference(prevDasys, currDasys);
            List<PortfolioModel> d4Difference = checkDifference(prevD4, currD4);
            List<PortfolioModel> mmDifference = checkDifference(prevMm, currMm);

            Dealers dealers = new Dealers();

            dealers.D1 = d1Difference;
            dealers.Dasys = dasysDifference;
            dealers.D4 = d4Difference;
            dealers.Mm = mmDifference;

            return dealers;
        }

        private static List<PortfolioModel> checkDifference(List<PortfolioModel> prev, List<PortfolioModel> curr)
        {
            List<PortfolioModel> diff = new List<PortfolioModel>();

            foreach (PortfolioModel item in curr)
            {
                diff.Add(item);
            }

            foreach (PortfolioModel item in prev)
            {
                int position = Convert.ToInt32(item.Position);
                if (position > 0)
                {
                    var zeroOutItem = curr.Where(o => o.SecurityID.Equals(item.SecurityID));
                    if (zeroOutItem.Count() == 0)
                    {
                        item.Position = "0";
                        item.AvgCost = "0";
                        diff.Add(item);
                    }
                }
        }

            return diff;
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
}

