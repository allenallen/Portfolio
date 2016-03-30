using PortfolioController;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class StockXDBController
    {


        public List<MatchedOrder> GetReport1()
        {
            var list = new List<MatchedOrder>();
            string dataSource = ConfigurationManager.AppSettings["dataSource"].ToString();
            string userId = ConfigurationManager.AppSettings["user"].ToString();
            string password = ConfigurationManager.AppSettings["password"].ToString();
            string connectionString = String.Format("Data Source={0};User ID={1};Password={2};",dataSource,userId,password);
            using (SqlConnection connection =
            new SqlConnection(connectionString))
            {
                string query = File.ReadAllText(ConfigurationManager.AppSettings["TechniFilledRecordsSQL"].ToString());
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(query, connection);
                Console.WriteLine("Connection succeeded");
                //command.Parameters.AddWithValue("@pricePoint", paramValue);

                // Open the connection in a try/catch block. 
                // Create and execute the DataReader, writing the result
                // set to the console window.
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    MatchedOrder order = null;
                    while (reader.Read())
                    {
                        //Console.WriteLine(reader["QUANTITY"].GetType());
                        
                        int fieldCount = reader.FieldCount;
                        order = new MatchedOrder();
                        order.UserId = (string)reader["USER ID"];
                        order.AccountCode = (string)reader["ACCOUNT CODE"];
                        order.OrderDatetime = (DateTime)reader["ORDER DATETIME"];
                        order.MatchDate = order.OrderDatetime.Value.ToString("M/d/yyyy");
                        order.Side = (string)reader["SIDE"];
                        order.StockCode = (string)reader["STOCKCODE"];
                        order.BoardLot = (string)reader["BOARDLOT"];
                        order.Price = (decimal?)reader["PRICE"];
                        //var q = (long)reader["QUANTITY"];
                        var q = reader.GetInt64(7);
                        order.Quantity = Convert.ToInt32(q);
                        //order.Quantity = Convert.ToInt32(reader["QUANTITY"]);
                        //order.Side = (string)reader["SIDE"];
                        //for (int i = 0; i < fieldCount; i++)
                        //{
                        //    Console.WriteLine(reader.GetName(i) + ": " + reader[i].ToString());
                        //}
                        list.Add(order);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return list;
            }
        }
        private Client GetClient(string accountCode)
        {
            using(var dbContext = new StockXDataBaseEntities())
            {
                Client c = (from client in dbContext.Client
                           where client.AccountCode == accountCode
                           select client).SingleOrDefault();
                
                return c;
            }
        }
        public void GenerateSOA(DateTime date)
        {
            List<MatchedOrder> list = GetAllRecords();
            var accounts = list.Select(o => o.AccountCode).Distinct().OrderBy(o => o).ToList();
            string soaString = ProcessListForSOA(list, accounts);

            var soaPath = ConfigurationManager.AppSettings["txtFilesPath"].ToString() + "soa.txt";
            var soaPathwDate = String.Format("{0}soa-{1}-{2}-{3}.txt", ConfigurationManager.AppSettings["txtFilesBackupPath"].ToString(), date.Year, date.Month, date.Day);

            File.WriteAllText(soaPath, soaString);
            File.WriteAllText(soaPathwDate, soaString);
        }
        private string ProcessListForSOA(List<MatchedOrder> allRecords, List<string> accounts)
        {
            StringBuilder text = new StringBuilder();
            foreach (string account in accounts)
            {
                Client client = GetClient(account);
                
                if (!account.StartsWith("TEST"))
                {
                    decimal initialCapital = (decimal)client.InitialCapital;
                    
                    var transactions = allRecords.Where(o => o.AccountCode == account).ToList();
                    var datesOfTransactions = transactions.Select(o => o.MatchDate).Distinct().ToList();
                    foreach (string dates in datesOfTransactions)
                    {
                        var listOfStocksPerDate = transactions.Where(o => o.MatchDate == dates).Select(o => o.StockCode).Distinct().ToList();
                        foreach (string stock in listOfStocksPerDate)
                        {
                            List<MatchedOrder> buyTransactionsAccountPerStockPerDate =
                                        transactions.Where(o => o.MatchDate == dates && o.StockCode == stock && o.Side == "B").ToList();

                            List<MatchedOrder> sellTransactionsAccountPerStockPerDate =
                                        transactions.Where(o => o.MatchDate == dates && o.StockCode == stock && o.Side == "S").ToList();

                            WriteToStringBuyTransactions(text, client, buyTransactionsAccountPerStockPerDate);
                            WriteToStringSellTransactions(text, client, sellTransactionsAccountPerStockPerDate);
                        }
                    }
                }
            }
            text.AppendLine("");
            text.AppendLine("");
            return text.ToString();
        }
        private void WriteToStringSellTransactions(StringBuilder text, Client client,List<MatchedOrder> list)
        {
            string lines = null;
            StringBuilder s = new StringBuilder();
            int totalVolume = 0;
            decimal avgPrice = 0M;
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 0 && list.Count == 1)
                    {
                        string padded = list[i].MatchedOrderID.ToString().PadLeft(5,'0');
                        client.InitialCapital = (decimal)(client.InitialCapital + (list[i].Quantity * list[i].Price));
                        s.Append(String.Format("{0};{1};SI-1{2};Sale of {3} {4} shares @ {5};-{6};{7};{8};{9}", list[i].AccountCode, list[i].MatchDate,
                            padded, list[i].StockCode, list[i].Quantity, list[i].Price, list[i].Quantity, list[i].Price, list[i].Quantity * list[i].Price, client.InitialCapital));
                        
                    }
                    else if(i == 0 && list.Count > 1)
                    {
                        string padded = list[i].MatchedOrderID.ToString().PadLeft(5, '0');
                        s.Append(String.Format("{0};{1};SI-1{2};Sale of {3} {4} shares @ {5}", list[i].AccountCode, list[i].MatchDate,
                            padded, list[i].StockCode, list[i].Quantity, list[i].Price));
                    }
                    else if (i > 0 && i != list.Count - 1)
                    {
                        s.Append(String.Format(", {0} shares @ {1}", list[i].Quantity, list[i].Price));
                    }
                    else
                    {
                        totalVolume = (int)list.Sum(o => o.Quantity);
                        avgPrice = (decimal)(list.Sum(o => o.Price)/list.Count);
                        client.InitialCapital = (decimal)(client.InitialCapital + (totalVolume * avgPrice));
                        s.Append(String.Format(", {0} shares @ {1};-{2};{3};{4};{5}", list[i].Quantity, list[i].Price,totalVolume, avgPrice
                                                                                    , totalVolume * avgPrice, client.InitialCapital));
                       
                    }
                }
                lines = s.ToString();
                if (lines != null) text.AppendLine(lines);
            }
        }
        private void WriteToStringBuyTransactions(StringBuilder text, Client client, List<MatchedOrder> list)
        {
            string lines = null;
            StringBuilder s = new StringBuilder();
            int totalVolume = 0;
            decimal avgPrice = 0M;
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 0 && list.Count == 1)
                    {
                        string padded = list[i].MatchedOrderID.ToString().PadLeft(5, '0');
                        client.InitialCapital = (decimal)(client.InitialCapital - (list[i].Quantity * list[i].Price));
                        s.Append(String.Format("{0};{1};BI-1{2};Purchase of {3} {4} shares @ {5};{6};{7};{8};{9}", list[i].AccountCode, list[i].MatchDate,
                            padded, list[i].StockCode, list[i].Quantity, list[i].Price, list[i].Quantity, list[i].Price, list[i].Quantity * list[i].Price, client.InitialCapital));
                       
                    }
                    else if (i == 0 && list.Count > 1)
                    {
                        string padded = list[i].MatchedOrderID.ToString().PadLeft(5, '0');
                        s.Append(String.Format("{0};{1};BI-1{2};Purchase of {3} {4} shares @ {5}", list[i].AccountCode, list[i].MatchDate,
                            padded, list[i].StockCode, list[i].Quantity, list[i].Price));
                    }
                    else if (i > 0 && i != list.Count - 1)
                    {
                        s.Append(String.Format(", {0} shares @ {1}", list[i].Quantity, list[i].Price));
                    }
                    else
                    {
                        totalVolume = (int)list.Sum(o => o.Quantity);
                        avgPrice = (decimal)(list.Sum(o => o.Price) / list.Count);
                        client.InitialCapital = (decimal)(client.InitialCapital - (totalVolume * avgPrice));
                        s.Append(String.Format(", {0} shares @ {1};{2};{3};{4};{5}", list[i].Quantity, list[i].Price, totalVolume, avgPrice
                                                                                    , totalVolume * avgPrice, client.InitialCapital));
                       
                    }
                }
                lines = s.ToString();
                if (lines != null) text.AppendLine(lines);
            }
        }
        private string FormatSell(MatchedOrder o)
        {
            var line = String.Format("{0};{1};BI-{2};Sale of {3} {4} shares @ {5}", o.AccountCode.Trim(), o.MatchDate
                                        , o.StockCode, o.Quantity, o.Price);
            return line;
        }
        private string FormatBuy(MatchedOrder o)
        {
            var line = String.Format("{0};{1};Purchase of {2} {3} shares @ {4}", o.AccountCode.Trim(), o.MatchDate
                                        ,o.StockCode,o.Quantity,o.Price);
            return line;
        }
        public void GeneratePosCost(DateTime date)
        {
//            1. Let's create 3 accounts in table.  With different cases:
//2. Let's create a script that will get only the last record of the matched order per account per stock.  this step is right after all 
//avg cost calculation is done.
//2. We get a list of all matched orders for today.
//2.a. Get all records sorted by account and stock and date.
//2.a.a  Loop thru records
//2.b. via LinQ, get only per stock per account.
//2.c. Sort by date. Grab only the first record
            //2.d. append it to postcost. Save two files. One has today's date poscost-<date>.txt and one just latest poscost.txt
            DateTime yesterday = date.AddDays(-1);
            DateTime tomorrow = date.AddDays(1);
            using (var dbContext = new StockXDataBaseEntities())
            {
                //var tempList = (from match in dbContext.MatchedOrder
                //                where (match.OrderDatetime < tomorrow && match.OrderDatetime > yesterday)
                //                orderby match.AccountCode, match.StockCode, match.OrderDatetime
                //                select match).ToList();

                var tempList = (from match in dbContext.Portfolio where match.StockCode != "TEST"
                                select match).ToList();

                if (tempList.Count == 0) return;
                var accounts = tempList.Select(o => o.AccountCode).Distinct();
                //foreach (var temp in tempList)
                //{
                //    Console.WriteLine(temp.AccountCode);
                //    Console.WriteLine(temp.AvgCost);
                //}
                var s = new StringBuilder();
                foreach (var i in accounts)
                {
                    var account = tempList.Where(o => o.AccountCode == i);
                    var stocks = account.Select(o => o.StockCode).Distinct();
                    var poscosListPerAccount = new List<string>();

                    foreach(var stock in stocks)
                    {
                        var list = account.Where(o => o.StockCode == stock);
                        //var list = transactionList.OrderByDescending(o => o.OrderDatetime).ToList();
                        var item = list.First();

                        s.AppendLine(String.Format("{0};{1};{2};{3}", item.AccountCode.Trim(), item.StockCode.Trim(), item.Shares, item.AvgCost));
                      
                    }
                }
                s.AppendLine("");
                s.AppendLine("");
                string path = ConfigurationManager.AppSettings["txtFilesPath"].ToString() + "poscost.txt";
                string pathWithDate = String.Format("{0}poscost-{1}-{2}-{3}.txt", ConfigurationManager.AppSettings["txtFilesBackupPath"].ToString(), date.Month, date.Day, date.Year);
                
                File.WriteAllText(path,s.ToString());
                File.WriteAllText(pathWithDate, s.ToString());
                //var firstRecord = tempList.First();
                //if(tempList.Count == 0)
                //    MatchedOrder order = new MatchedOrder();
                //var lastRecord = tempList.Last();

                //list.Add(firstRecord);
                //list.Add(lastRecord);
                //return list;
            }
        }
        public void Test()
        {
            var pcTable = DataBuilder.GetPosCostTableData();
            foreach (var item in pcTable)
            {
                Console.WriteLine("Account Code: "+item.AccountCode);
                Console.WriteLine("Volume: " + item.Volume);
                Console.WriteLine("Stock Code: " + item.StockCode);
                Console.WriteLine("Average Cost: " + item.AverageCost);
                Console.WriteLine("-------");
            }
            //using (StockXDataBaseEntities db = new StockXDataBaseEntities())
            //{
            //    MatchedOrder m = new MatchedOrder() {AccountCode="OTA12345",BoardLot="MAIN",OrderDatetime=DateTime.Now,Price=0.72m,
            //                                            Quantity=1000,Side="B",StockCode="NOW",UserId="jtinimbang", MatchDate=DateTime.Now.ToShortDateString()};
             
            //    string d = m.MatchDate;
            //    var orders = from a in db.MatchedOrders where a.StockCode == m.StockCode && a.AccountCode == m.AccountCode
            //                 && a.MatchDate == d && a.Price == m.Price && a.Quantity == m.Quantity
            //                 select a;
            //    List<MatchedOrder> list = orders.ToList<MatchedOrder>();
            //    if (list.Count == 0)
            //    {
            //        db.MatchedOrders.Add(m);
            //    }
            //    else
            //    {
            //        var item = list.First<MatchedOrder>();
            //        Console.WriteLine(item.AccountCode);
            //        Console.WriteLine(item.OrderDatetime.Value.ToShortDateString());
            //    }
               
            //    db.SaveChanges();
                
            //    Console.WriteLine(list.Count);
            //    foreach (var item in list)
            //    {
            //        Console.WriteLine(item.MatchedOrderID);
            //    }
            //}

        }
        public void CheckLastRecord(MatchedOrder lastRecord,List<MatchedOrder>current)
        {
            if (lastRecord != null && current.Count > 0 && lastRecord.MatchedOrderID != current[0].MatchedOrderID)
            {
                current.Reverse();
                current.Add(lastRecord);
                current.Reverse();
            }
        }
        public void ProcessCash(List<MatchedOrder> todayList, DateTime date)
        {

            var query = (from t in todayList
                         group t by new { t.AccountCode }
                             into grp
                             select new { grp.Key.AccountCode }).Distinct().ToList();

            //var queryPerAccountPerStock = (from t in todayList
            //             group t by new { t.StockCode, t.AccountCode }
            //                 into grp
            //                 select new { grp.Key.AccountCode, grp.Key.StockCode }).Distinct().ToList();



            var cashList = new List<Cash>();
            foreach (var item in query)
            {
                //var matchedOrders = GetMatchedOrdersByAccountCode(item.AccountCode, date);
                //CalculateCash(todayList);
                Console.WriteLine("finished calculating cash per account");
                //switch to return to cash only
                Cash cash = CalculateCash(item.AccountCode,date.ToShortDateString(), todayList);
                if (cash != null)
                {
                    cashList.Add(cash);
                }
            }
            SaveToDB(cashList);
        }
        public void ProcessOrdersByDate(List<MatchedOrder> todayList, DateTime date)
        {
            var query = (from t in todayList
                         group t by new { t.StockCode, t.AccountCode }
                             into grp
                             select new { grp.Key.AccountCode, grp.Key.StockCode }).Distinct().ToList();
            //var accounts = todayList.GroupBy(x => new {x.AccountCode,x.StockCode }).Select(o => o.;

            //var cashList = new List<Cash>();
            foreach (var item in query)
            {
                Console.WriteLine("account:{0}, stock code:{1}", item.AccountCode, item.StockCode);
                var lastRecord = GetLastRecord(item.AccountCode, item.StockCode, date);

                var matchedOrders = GetMatchedOrdersByAccountCodeAndStockCodeByDate(item.AccountCode, item.StockCode, date);

                HandleLastRecord(item.AccountCode, item.StockCode, date, matchedOrders);

                var clientAvgCost = TransformMatchedOrderToClientAverageCostList(matchedOrders);
                CalculateAvgCost(matchedOrders, clientAvgCost);
                //switch to return to cash only
                //cashList = CalculateCash(matchedOrders);
                SaveToDB(matchedOrders);
            }
        }
        public void ProcessAllOrdersByDate(List<MatchedOrder> todayList,DateTime date)
        {

            var query = (from t in todayList
                         group t by new { t.StockCode, t.AccountCode }
                             into grp
                             select new { grp.Key.AccountCode, grp.Key.StockCode }).Distinct().ToList();
            //var accounts = todayList.GroupBy(x => new {x.AccountCode,x.StockCode }).Select(o => o.;

            //var cashList = new List<Cash>();
            foreach (var item in query)
            {
                Console.WriteLine("account:{0}, stock code:{1}", item.AccountCode, item.StockCode);
                var lastRecord = GetLastRecord(item.AccountCode, item.StockCode,date);

                var matchedOrders = GetPreviousMatchedOrdersByAccountCodeAndStockCode(item.AccountCode, item.StockCode, date);
                
                var clientAvgCost = TransformMatchedOrderToClientAverageCostList(matchedOrders);
                CalculateAvgCost(matchedOrders, clientAvgCost);
                //switch to return to cash only
                //cashList = CalculateCash(matchedOrders);
                SaveToDB(matchedOrders);
            }
            //SaveToDB(cashList);
            
            //s.GeneratePosCost(date);
            //s.GenerateSOA(date);
        }
        private Cash LookupCashByAccount(String accountCode, String matchdate)
        {
            //var cash = new List<Cash>();
            Cash cash = null;
           
            using (var dbContext = new StockXDataBaseEntities())
            {
                cash = (from match in dbContext.Cash
                        where match.AccountCode == accountCode
                        select match).SingleOrDefault();

                //cash = (from match in dbContext.Cash
                //                               select match).ToList();

            }
            return cash;
        }
        public Cash CalculateCash(string accountCode, string matchDate, List<MatchedOrder> matchedOrders)
        {
            Cash cash = null;
            if (matchedOrders.Count == 0) return null;

            var query = (from t in matchedOrders where t.AccountCode == accountCode select t).ToList<MatchedOrder>();

            cash = LookupCashByAccount(accountCode, matchDate);
            //get only stocks per account


            foreach (var matchedOrder in query)
            {
                if (cash != null)
                {
                    Console.WriteLine("lookup account:{0}, stock:{1}", matchedOrder.AccountCode, matchedOrder.StockCode);
                    var newCash = (matchedOrder.Side == "BUY" || matchedOrder.Side == "B")
                                    ? cash.Amount - (matchedOrder.Price * matchedOrder.Quantity)
                                    : cash.Amount + (matchedOrder.Price * matchedOrder.Quantity);
                    cash.Amount = Convert.ToDecimal(newCash);
                    cash.LastUpdatedDate = matchedOrder.MatchDate;

                }
            }
            return cash;
        }
        public List<Cash> CalculateCashTemp(List<MatchedOrder> matchedOrders){
            Cash accountCash = null;
            using (var dbContext = new StockXDataBaseEntities())
            {
                List<Cash> cashList = (from match in dbContext.Cash
                                       select match).ToList();
                foreach (var matchedOrder in matchedOrders)
                {
                    List<Cash> accountCashList = cashList.Where(o => o.AccountCode == matchedOrder.AccountCode && o.LastUpdatedDate != matchedOrder.MatchDate).ToList();
                    if (accountCashList.Count != 0)
                    {
                        accountCash = accountCashList.Single();
                    }
                    if (accountCash != null)
                    {
                        var newCash = (matchedOrder.Side == "BUY" || matchedOrder.Side == "B")
                                        ? accountCash.Amount - (matchedOrder.Price * matchedOrder.Quantity)
                                        : accountCash.Amount + (matchedOrder.Price * matchedOrder.Quantity);
                        accountCash.Amount = Convert.ToDecimal(newCash);
                        accountCash.LastUpdatedDate = matchedOrder.MatchDate;
                    }
                }
            }
            return null;

        }
        private List<Cash> CalculateCashList(List<MatchedOrder> matchedOrders)
        {
            var cash = new List<Cash>();
            using (var dbContext = new StockXDataBaseEntities())
            {
                List<Cash> cashList = (from match in dbContext.Cash
                            select match).ToList();
              foreach (var matchedOrder in matchedOrders)
                {
                    List<Cash> accountCashList = cashList.Where(o => o.AccountCode == matchedOrder.AccountCode && o.LastUpdatedDate != matchedOrder.MatchDate).ToList();
                    Cash accountCash = null;
                  if (accountCashList.Count != 0)
                    {
                        accountCash = accountCashList.Single(); 
                    }
                  if (accountCash != null)
                    {
                        var newCash = (matchedOrder.Side == "BUY" || matchedOrder.Side == "B")
                                        ? accountCash.Amount - (matchedOrder.Price * matchedOrder.Quantity)
                                        : accountCash.Amount + (matchedOrder.Price * matchedOrder.Quantity);
                        accountCash.Amount = Convert.ToDecimal(newCash);
                        accountCash.LastUpdatedDate = matchedOrder.MatchDate;
                        cash.Add(accountCash);
                    }
                }
            }
            
            return cash;
        }
        public MatchedOrder GetLastRecord(string otCode, string stockCode,DateTime date)
        {
            List<MatchedOrder> list = new List<MatchedOrder>();
            DateTime newDate = new DateTime(date.Year, date.Month, date.Day);
            MatchedOrder lastRecord = null;
            using (var dbContext = new StockXDataBaseEntities())
            {
                var tempList = (from match in dbContext.MatchedOrder
                                         where (match.AccountCode == otCode && match.StockCode == stockCode
                                                    && match.OrderDatetime < newDate)
                                         orderby match.OrderDatetime
                                         select match).ToList();

                //var firstRecord = tempList.First();
                //if(tempList.Count == 0)
                //    MatchedOrder order = new MatchedOrder();
                if (tempList.Count > 0)
                {
                    lastRecord = tempList.Last();
                }

            }
            return lastRecord;

        }
        public List<MatchedOrder> GetMatchedOrdersByAccountCode(string otCode, DateTime baseDate)
        {

            using (var dbContext = new StockXDataBaseEntities())
            {
                List<MatchedOrder> list = null;
                var matchedOrderTable = (from match in dbContext.MatchedOrder
                                         where (match.AccountCode == otCode && match.OrderDatetime <= baseDate)
                                         orderby match.OrderDatetime
                                         select match);

                //if (matchedOrderTable.Count() > 2)
                //{
                //   matchedOrderTable =  matchedOrderTable.Take(2);
                //}
                //               // select  top 2 from MatchedOrder where orderdatetime <= '6/30/2015'
                list = matchedOrderTable.OrderBy(o => o.OrderDatetime).ToList();

                //list.Reverse();
                return list;
            }
        }
        public List<MatchedOrder> GetPreviousMatchedOrdersByAccountCodeAndStockCode(string otCode, string stockCode, DateTime baseDate)
        {
          
            using (var dbContext = new StockXDataBaseEntities())
            {
                List<MatchedOrder> list = null;
                var matchedOrderTable = (from match in dbContext.MatchedOrder
                                         where (match.AccountCode == otCode && match.StockCode == stockCode && match.OrderDatetime <= baseDate)
                                         orderby match.OrderDatetime
                                         select match);

                //if (matchedOrderTable.Count() > 2)
                //{
                //   matchedOrderTable =  matchedOrderTable.Take(2);
                //}
                //               // select  top 2 from MatchedOrder where orderdatetime <= '6/30/2015'
                list = matchedOrderTable.OrderBy(o => o.OrderDatetime).ToList();

                //list.Reverse();
                return list;
            }
        }
        public List<MatchedOrder> GetMatchedOrdersByAccountCodeAndStockCodeByDate(string otCode, string stockCode, DateTime baseDate)
        {
            string matchDate = baseDate.ToShortDateString();
            using (var dbContext = new StockXDataBaseEntities())
            {
                List<MatchedOrder> list = null;
                var matchedOrderTable = (from match in dbContext.MatchedOrder
                                         where (match.AccountCode == otCode && match.StockCode == stockCode && match.MatchDate == matchDate)
                                         select match);

                //if (matchedOrderTable.Count() > 2)
                //{
                //   matchedOrderTable =  matchedOrderTable.Take(2);
                //}
                //               // select  top 2 from MatchedOrder where orderdatetime <= '6/30/2015'
                list = matchedOrderTable.OrderBy(o => o.OrderDatetime).ToList();

                //list.Reverse();
                return list;
            }
        }
        public List<MatchedOrder> GetAllRecords()
        {
            using (var dbContext = new StockXDataBaseEntities())
            {
                var orders = from match in dbContext.MatchedOrder select match;
                return orders.ToList();
            }
        }
        public List<MatchedOrder> GetAllTodayRecords()
        {
            string today = DateTime.Now.ToShortDateString();
            using (var dbContext = new StockXDataBaseEntities())
            {
                var orders = from match in dbContext.MatchedOrder 
                                where match.MatchDate == today
                             select match;
                return orders.ToList();
            }
        }
        public List<MatchedOrder> GetAllPreviousRecordsByDate(DateTime date)
        {
            //string today = date.ToShortDateString();
            using (var dbContext = new StockXDataBaseEntities())
            {
                //date = date.AddDays(1);
                var orders = from match in dbContext.MatchedOrder
                             where match.OrderDatetime <= date
                             select match;
                return orders.ToList();
            }
        }
        public List<MatchedOrder> GetAllRecordsByDate(DateTime date)
        {
            string today = date.ToShortDateString();
            using (var dbContext = new StockXDataBaseEntities())
            {
                var orders = from match in dbContext.MatchedOrder
                             where match.MatchDate == today
                             select match;
                return orders.ToList();
            }
        }
        public List<Portfolio> GetPortfolioRecordsByAccountcode(string accountCode)
        {
            using (var dbContext = new StockXDataBaseEntities())
            {
                var orders = from match in dbContext.Portfolio
                             where match.AccountCode == accountCode
                             select match;
                return orders.ToList();
            }
        }
        public List<ClientAverageCost> TransformMatchedOrderToClientAverageCostList(List<MatchedOrder> list)
        {
            List<ClientAverageCost> clientList = new List<ClientAverageCost>();

            

            foreach (MatchedOrder item in list)
            {
                ClientAverageCost client = new ClientAverageCost()
                {
                    Date = item.MatchDate,
                    Cost = item.Price,
                    Side = item.Side,
                    Volume = item.Quantity,
                    BuyPrice = item.Price * item.Quantity,
                    SellPrice = item.Price * item.Quantity,
                    SumOfNetPrice = (item.SumOfNetPrice == null ) ? item.Price * item.Quantity : item.SumOfNetPrice,
                    NetVolume = (item.NetVolume == null) ? item.Quantity : item.NetVolume,
                    AverageCost = (item.AvgCost == null) ? ((item.Price * item.Quantity) / item.Quantity) : item.AvgCost,
                    MatchedOrderID = item.MatchedOrderID
                };
                clientList.Add(client);
            }

            if (clientList.Count == 1)
            {
                var c = new ClientAverageCost()
                {
                    Date = "",
                    Side = "",
                    Cost = 0,
                    Volume = 0,
                    BuyPrice = 0,
                    //BuyPrice = 0,
                    SellPrice = 0,
                    NetVolume = 0,
                    SumOfNetPrice = 0,
                    AverageCost = 0
                };
                clientList.Add(c);
            }

            return clientList;
        }
        public void UpdateMatchedOrder(ClientAverageCost c )
        {
            // you get matched order sa db using the right key
            
            // then you set avg cost from c.avg cost
            //save matche order context.
        }
        public void Save(MatchedOrder order)
        {
            //MatchedOrder tempOrder = GetMatchedOrder(order.AccountCode, order.StockCode, order.OrderDatetime);
            
            //if (tempOrder != null)
            //{
            //    //update
            //}
            //else
            //{
            //    //insert
            //}
        }
        public void CalculateAvgCost(List<MatchedOrder> mList, List<ClientAverageCost> currentList)
        {

            AverageCostCalculator.CalculateAC(mList,currentList);
        }
        public void CalculateAvgCost(List<ClientAverageCost> currentList)
        {

            AverageCostCalculator.CalculateAC(currentList);
        }
        public void SaveNewListToDB(List<MatchedOrder> list)
        {
            using (var dbContext = new StockXDataBaseEntities())
            {
                //get latest based on ID.
                //MatchedOrder matchOrder = null;
                foreach (var mOrder in list)
                {
                    var matchedOrder = (from match in dbContext.MatchedOrder
                                        where (match.Side == mOrder.Side && match.OrderDatetime == mOrder.OrderDatetime 
                                        && match.AccountCode == mOrder.AccountCode && match.StockCode == mOrder.StockCode)
                                        select match).SingleOrDefault();

                    if (matchedOrder == null)
                    {
                        matchedOrder = new MatchedOrder();

                        dbContext.MatchedOrder.Add(matchedOrder);

                    }
                    if (matchedOrder != null)
                    {
                        matchedOrder.Price = mOrder.Price;
                        
                        matchedOrder.Side = mOrder.Side.Trim();
                        matchedOrder.AccountCode = mOrder.AccountCode.Trim();
                        matchedOrder.UserId = mOrder.UserId.Trim();
                        matchedOrder.OrderDatetime = mOrder.OrderDatetime;
                        matchedOrder.BoardLot = mOrder.BoardLot;
                        matchedOrder.MatchDate = mOrder.MatchDate.Trim();
                        matchedOrder.Quantity = mOrder.Quantity;
                        matchedOrder.StockCode = mOrder.StockCode.Trim();
                        //Console.WriteLine("save to db  old id: {2},  avg cost:{1} new avg cost:{0} ", mOrder.AvgCost, matchedOrder.AvgCost, matchedOrder.MatchedOrderID);
                        //matchedOrder.AvgCost = mOrder.AvgCost;
                        //matchedOrder.SumOfNetPrice = mOrder.SumOfNetPrice;
                        //matchedOrder.NetVolume = mOrder.NetVolume;
                        //matchedOrder.NetPrice = mOrder.Price * mOrder.Quantity;
                    }
                    dbContext.SaveChanges();

                }
            }
        }
        public void SaveToDB(List<MatchedOrder> list)
        {
            using (var dbContext = new StockXDataBaseEntities())
            {
                //get latest based on ID.
                foreach(var mOrder in list)
                {
                    var matchedOrder = (from match in dbContext.MatchedOrder
                                        where (match.MatchedOrderID == mOrder.MatchedOrderID)
                                        select match).Single();

                    if (matchedOrder != null)
                    {
                        Console.WriteLine("save to db  old id: {2},  avg cost:{1} new avg cost:{0} ", mOrder.AvgCost, matchedOrder.AvgCost, matchedOrder.MatchedOrderID);
                        matchedOrder.AvgCost = mOrder.AvgCost;
                        matchedOrder.SumOfNetPrice = mOrder.SumOfNetPrice;
                        matchedOrder.NetVolume = mOrder.NetVolume;
                        matchedOrder.NetPrice = mOrder.Price * mOrder.Quantity;
                        dbContext.SaveChanges();
                    }
                }
            }
        }
        public void SaveToDB(List<Cash> list)
        {
            using (var dbContext = new StockXDataBaseEntities())
            {
                //get latest based on ID.
                foreach (var cashAccount in list)
                {
                    SavetoCash(dbContext, cashAccount);
                    //SavetoCashHistory(dbContext, cashAccount);
                    //save also in history table


                }
            }
        }
        private static void SavetoCashHistory(StockXDataBaseEntities dbContext, Cash cashAccount)
        {
            var cash = (from match in dbContext.CashHistory
                        where (match.AccountCode == cashAccount.AccountCode && match.LastUpdatedDate == cashAccount.LastUpdatedDate)
                        select match).SingleOrDefault<CashHistory>();
            if (cash == null)
            {
                cash = new CashHistory();

                dbContext.CashHistory.Add(cash);

            }

            if (cash != null)
            {
                //Console.WriteLine("save to db  old id: {2},  avg cost:{1} new avg cost:{0} ", mOrder.AvgCost, matchedOrder.AvgCost, matchedOrder.MatchedOrderID);
                cash.AccountCode = cashAccount.AccountCode;
                cash.Amount = cashAccount.Amount;
                cash.Margin = cashAccount.Margin;
                cash.Withdrawable = cashAccount.Withdrawable;
                cash.LastUpdatedDate = cashAccount.LastUpdatedDate;
                dbContext.SaveChanges();
            }
        }
        private static void SavetoCash(StockXDataBaseEntities dbContext, Cash cashAccount)
        {
            var cash = (from match in dbContext.Cash
                        where (match.AccountCode == cashAccount.AccountCode && match.LastUpdatedDate != cashAccount.LastUpdatedDate)
                        select match).SingleOrDefault<Cash>();

            if (cash != null)
            {
                
                //Console.WriteLine("save to db  old id: {2},  avg cost:{1} new avg cost:{0} ", mOrder.AvgCost, matchedOrder.AvgCost, matchedOrder.MatchedOrderID);
                cash.AccountCode = cashAccount.AccountCode;
                cash.Amount = cashAccount.Amount;
                cash.Margin = cashAccount.Margin;
                cash.Withdrawable = cashAccount.Withdrawable;
                cash.LastUpdatedDate = cashAccount.LastUpdatedDate;
                dbContext.SaveChanges();

                SavetoCashHistory(dbContext, cash);
            }

        }
        public void GenerateCash(DateTime date)
        {
            List<Cash> cashList = GetCashData(date);
            var s = new StringBuilder();
            foreach (Cash o in cashList)
            {
                if(!o.AccountCode.StartsWith("TEST"))
                s.AppendLine(String.Format("{0};{1};{2};{3}", o.AccountCode.Trim(), o.Amount, o.Margin, o.Withdrawable));
            }
            s.AppendLine("");
            s.AppendLine("");
            var soaPath = ConfigurationManager.AppSettings["txtFilesPath"].ToString() + "cash.txt";
            var soaPathwDate = String.Format("{0}cash-{1}-{2}-{3}.txt", ConfigurationManager.AppSettings["txtFilesBackupPath"].ToString(), date.Year, date.Month, date.Day);

            File.WriteAllText(soaPath, s.ToString());
            File.WriteAllText(soaPathwDate, s.ToString());
        }
        private List<Cash> GetCashData(DateTime date)
        {
            using (var dbContext = new StockXDataBaseEntities())
            {
                var cash = from match in dbContext.Cash select match;
                return cash.ToList();
            }
        }
        public void HandleLastRecord(string accountCode, string stockCode, DateTime baseDate, List<MatchedOrder> currentList)
        {
            MatchedOrder lastRecord = GetLastRecord(accountCode, stockCode, baseDate);
            CheckLastRecord(lastRecord, currentList);
        }
        public void BackupDB()
        {
            string db = ConfigurationManager.AppSettings["oldDBPath"].ToString();
            string newname = String.Format(ConfigurationManager.AppSettings["backupDBPath"], DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Year);
            System.IO.File.Copy(db, newname);
        }
        public Dictionary<string,decimal> GetUpdatedClosingPrices(List<string> stocks)
        {
             return StockPriceManager.GetStockClosingPrice(stocks);
        }
        public void UpdatePortfolio(List<MatchedOrder> list)
        {
            PortfolioManager.UpdatePortfolio(list);
        }
        public void UpdatePortfolioPrices(StockXDataBaseEntities dbContext,Dictionary<string, decimal> prices)
        {
            PortfolioManager.UpdatePortfolioPrices(dbContext,prices);
        }
        public List<Leaderboard> GetLeaders()
        {
            return LeaderBoardManager.GetLeaders();
        }
        public void RemoveFromMatchedOrdersTable(MatchedOrder order)
        {
            using (var dbContext = new StockXDataBaseEntities())
            {
                var matchedOrder = (from match in dbContext.MatchedOrder
                                    where match.AccountCode == order.AccountCode
                                    && match.BoardLot == "TESTZERO"
                                    select match).SingleOrDefault();
                dbContext.MatchedOrder.Remove(matchedOrder);
                dbContext.SaveChanges();
            }
        }
        public void SaveLeaderListAndGenerateFile(List<Leaderboard> list)
        {
            using(var dbContext = new StockXDataBaseEntities())
            {
                foreach(var item in list)
                {
                    var existing = (from entry in dbContext.Leaderboard
                                    where entry.AccountCode == item.AccountCode
                                    select entry).SingleOrDefault();
                    if (existing == null)
                    {
                        dbContext.Leaderboard.Add(item);
                    }
                    if (existing != null)
                    {
                        existing.PnL = item.PnL;
                    }
                    dbContext.SaveChanges();
                }
                LeaderBoardManager.GenerateLeadersFile(list);
            }
        }
    }
}
