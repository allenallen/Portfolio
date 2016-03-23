using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class LeaderBoardManager
    {
        public static List<Leaderboard> GetLeaders()
        {
            List<Client> list = ClientManager.GetAllClients();
            //calculate amount
            //combine leader board with client with inner join 
            List<Leaderboard> leaders = new List<Leaderboard>();
            List<Portfolio> pos = PortfolioManager.GetPortfolioList();
            foreach(var client in list){
                leaders.Add( CalculateScore(client,pos));
            }
            
            return leaders;
        }
        public static List<Leaderboard> GetLeaders(List<Client> list)
        {
            //List<Client> list = ClientManager.GetAllClients();
            //calculate amount
            //combine leader board with client with inner join 
            List<Leaderboard> leaders = new List<Leaderboard>();
            List<Portfolio> pos = PortfolioManager.GetPortfolioList();
            foreach (var client in list)
            {
                leaders.Add(CalculateScore(client, pos));
            }

            return leaders;
        }
        private static Leaderboard CalculateScore(Client c, List<Portfolio> pos)
        {
            var l = new Leaderboard();
            l.AccountCode = c.AccountCode;
            l.Capital = c.InitialCapital;
            var posByAccount = (from match in pos
                                     where match.AccountCode == c.AccountCode
                                     select match).ToList();
            
            decimal? amount = 0M;

            foreach (var item in posByAccount)
            {
                //amount += (item.Price - item.AvgCost)* item.Shares;
                amount += (item.Price) * item.Shares;
            }

            var cash = 0M;
            using (var dbContext = new StockXDataBaseEntities())
            {
                var accountCash = (from acct in dbContext.Cash
                                   where acct.AccountCode == c.AccountCode
                                   select acct).SingleOrDefault();
                cash = accountCash.Amount;
            }

            decimal? total = cash + amount;
            decimal? one = 1m;
            l.PnL = Convert.ToDouble((total / l.Capital) - one);
            //using (var dbContext = new StockXDataBaseEntities())
            //{

            //    var matchedOrderTable = (from match in dbContext.MatchedOrders
            //                             where (match.AccountCode == c.AccountCode)
            //                             orderby match.OrderDatetime
            //                             select match);

            //}
            return l;
        }
    }
}
