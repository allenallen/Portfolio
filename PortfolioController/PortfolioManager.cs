using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class PortfolioManager
    {
        public static Dictionary<string,decimal> GetCurrentPrices(List<String> list)
        {
            //list = ["mbt","ap","ac"];
            foreach(var item in list){
                
            }
            return null;
        }
        public static List<Portfolio> GetPortfolioList(){
            List<Portfolio> list = null;
            using (var dbContext = new StockXDataBaseEntities())
            {

                list = (from match in dbContext.Portfolio
                                         
                                         select match).ToList<Portfolio>();
            }
            return list;
        }
        // loop thru clients, check for matched order in match table, add stocks to port with buy and remove with sell
        // get current match order for each client, then update portfolio
        public static void UpdatePortfolio(List<MatchedOrder> matchedOrders)
        {
            if (matchedOrders.Count == 0) return;
            string[] stocks = matchedOrders.Select(o => o.StockCode).Distinct().ToArray();
            Portfolio portfolioEntry = null;
            using (var dbContext = new StockXDataBaseEntities())
            {
                foreach (var stock in stocks)
                {
                    portfolioEntry = new Portfolio();
                    var matchedOrdersPerStockBuy = matchedOrders.Where(o => o.StockCode == stock &&
                        o.Side == "B").ToList();
                    var matchedOrdersPerStockSell = matchedOrders.Where(o => o.StockCode == stock &&
                        o.Side == "S").ToList();
                    
                    int shares = 0;

                    shares = (int)matchedOrdersPerStockBuy.Sum(o => o.Quantity);
                    shares -= (int)matchedOrdersPerStockSell.Sum(o => o.Quantity);
                    portfolioEntry.AvgCost = matchedOrders.Where(o => o.StockCode == stock).Last().AvgCost;
                    portfolioEntry.AccountCode = matchedOrders.Last().AccountCode.Trim();
                    portfolioEntry.Price = matchedOrders.Where(o => o.StockCode == stock).Last().Price;
                    
                    portfolioEntry.Shares = shares;
                    portfolioEntry.StockCode = stock;

                    CheckIfPortfolioExistsThenAdd(dbContext,portfolioEntry);
                }
                
            }
        }

        private static void CheckIfPortfolioExistsThenAdd(StockXDataBaseEntities dbContext, Portfolio portfolioEntry)
        {
            var existing = (from entry in dbContext.Portfolio
                           where entry.AccountCode == portfolioEntry.AccountCode
                           && entry.StockCode == portfolioEntry.StockCode
                           select entry).SingleOrDefault();
            if (existing == null)
            {
                if (portfolioEntry.Shares > 0)
                {
                    dbContext.Portfolio.Add(portfolioEntry);
                }
               
                
            }
            if(existing != null)
            {
                existing.Shares = portfolioEntry.Shares;
                existing.AvgCost = portfolioEntry.AvgCost;
                if (existing.Shares == 0)
                {
                    dbContext.Portfolio.Remove(existing);
                }

            }
            dbContext.SaveChanges();
            
        }

        internal static void UpdatePortfolioPrices(StockXDataBaseEntities dbContext, Dictionary<string, decimal> prices)
        {
            var portfolio = (from entry in dbContext.Portfolio
                             select entry).ToList();
            foreach (var entry in portfolio)
            {
                if(entry.StockCode != "TEST")
                entry.Price = prices[entry.StockCode];
            }
        }
    }
}
