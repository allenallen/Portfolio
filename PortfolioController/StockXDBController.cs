using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class StockXDBController
    {
        
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
        public MatchedOrder[] GetMatchedOrders()
        {
            var matchedOrder = new MatchedOrder(){UserId="jtinimbang", AccountCode = "OTA12354"
                        , StockCode="NOW",Side="BUY",Quantity=100,Price=2.5M,OrderDatetime=DateTime.Now,BoardLot="a"};
            var matchedOrders = new List<MatchedOrder>(){ matchedOrder };
            return matchedOrders.ToArray();
        }
        public void ProcessOrders(MatchedOrder[] list)
        {
            CsvWrite.write(list);
        }
        public MatchedOrder GetMatchedOrder(string otCode, string stockCode, DateTime date)
        {
            return new MatchedOrder() {StockCode = stockCode,AccountCode=otCode,OrderDatetime=date};
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
        public void CalculateAvgCost(List<ClientAverageCost> list)
        {
            AverageCostCalculator.CalculateAC(list);
        }

    }
}
