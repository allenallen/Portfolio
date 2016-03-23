using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class AverageCostCalculator
    {

        ClientAverageCost client;
        bool BUY = false;
        bool SELL = false;
        decimal avgCost;

        public decimal AvgCost
        {
            get { return avgCost; }
            set { avgCost = value; }
        }

        public AverageCostCalculator() { }
        public AverageCostCalculator(String side,ClientAverageCost s)
        {
            this.client = s;
            if (side.Equals("BUY"))
            {
                this.BUY = true;
            }
            else if(side.Equals("SELL"))
            {
                this.SELL = true;
            }

            //this.avgCost = CalculateAC(this.client);
        }

        public static void CalculateAC2(List<ClientAverageCost> clients)
        {
            bool BUY = true;
            bool SELL = true;
        }

        public static void CalculateAC(List<MatchedOrder> mList, List<ClientAverageCost> clients)
        {
            var avgCost = 0;
            bool BUY = true;
            bool SELL = true;

            //if (clients.Count == 1)

            //    return;

            bool oneRecordOnly = clients[1].Date == "";
            bool twoRecords = !oneRecordOnly;




            if (oneRecordOnly)
            {
                SELL = clients[0].Side == "SELL" || clients[0].Side == "S";
                clients[0].BuyPrice = clients[0].Cost * clients[0].Volume;
                clients[0].SellPrice = clients[0].Cost * clients[0].Volume;
                clients[0].NetVolume = (SELL ? 0 : clients[0].Volume);
                clients[0].SumOfNetPrice = (SELL ? 0 : clients[0].BuyPrice);
                UpdateMatchedOrder(mList, clients[0]);

            }
            else
            {
                for (int i = 1; i < clients.Count; i++)
                {
                    SELL = clients[i].Side == "SELL" || clients[i].Side == "S";
                    clients[i].BuyPrice = clients[i].Cost * clients[i].Volume;
                    clients[i].SellPrice = clients[i].Cost * clients[i].Volume;
                    clients[i].NetVolume = (SELL ? clients[i - 1].NetVolume - clients[i].Volume : clients[i - 1].NetVolume + clients[i].Volume);
                    Console.WriteLine("side:{0}, Prev NETV:{1}, NETV:{2}", clients[i].Side, clients[i - 1].NetVolume, clients[i].NetVolume);
                    clients[i].SumOfNetPrice = (SELL ? clients[i -1].AverageCost * clients[i].NetVolume : clients[i - 1].SumOfNetPrice + clients[i].BuyPrice);
                    clients[i].AverageCost = (clients[i].NetVolume.Value == 0) ? 0m : clients[i].SumOfNetPrice.Value / clients[i].NetVolume.Value;
                    Console.WriteLine("side:{0}, Prev AvgCost:{1}, NETV:{2}, NetPrice: {3}", clients[i].Side, clients[i - 1].AverageCost, clients[i].NetVolume, clients[i].SumOfNetPrice);
                    UpdateMatchedOrder(mList,clients[i - 1]);
                    UpdateMatchedOrder(mList, clients[i]);
                }
                
            }

            
            //netV = Convert.ToInt16(oneRecordOnly) * (SELL ? 0 : clients[0].Volume)
            //    + Convert.ToInt16(twoRecords) * (SELL ? 0 : clients[1].Volume);

            //clients[0].NetVolume = netV;

            //first operand is 
            //var netP = Convert.ToInt16(oneRecordOnly) * (SELL ? 0 : clients[0].BuyPrice)
            //    + Convert.ToInt16(twoRecords) * (SELL ? clients[0].AverageCost * clients[1].NetVolume : clients[0].BuyPrice + clients[1].BuyPrice);

            //clients[1].SumOfNetPrice = netP;

            foreach(ClientAverageCost client in clients)
            {
                //client.SumOfNetPrice = netP;
                client.AverageCost = (client.NetVolume.Value == 0) ? 0m : client.SumOfNetPrice.Value / client.NetVolume.Value;
            }
            //client[0].AverageCost = (client[0].NetVolume.Value == 0) ? 0m : client[0].SumOfNetPrice.Value / client[0].NetVolume.Value;
            //client[1].AverageCost = 
            int buy = Convert.ToInt16(BUY);
            int sell = Convert.ToInt16(SELL);
            
            //x and y = formula
            int x = 1;
            int y = 1;

            //avgCost = buy * (x) + sell*(y);

            //return client;
        }

        private static void UpdateMatchedOrder(List<MatchedOrder> list, ClientAverageCost clientAverageCost)
        {
            var item = list.Where(o => o.MatchedOrderID == clientAverageCost.MatchedOrderID).Single();

            if (item != null)
            {
                item.NetVolume = clientAverageCost.NetVolume;
                item.SumOfNetPrice = clientAverageCost.SumOfNetPrice;
                item.AvgCost = clientAverageCost.AverageCost;
                Console.WriteLine("updated list: {0}, {1}", item.MatchedOrderID, item.AvgCost);
            }
            else
            {
                Console.WriteLine("item null: ", clientAverageCost.MatchedOrderID);
            }
           
        }
        public static void CalculateAC(List<ClientAverageCost> clients)
        {
            var avgCost = 0;
            bool BUY = true;
            bool SELL = true;

            //if (clients.Count == 1)

            //    return;

            bool oneRecordOnly = clients[1].Date == "";
            bool twoRecords = !oneRecordOnly;




            if (oneRecordOnly)
            {
                SELL = clients[0].Side == "SELL";
                clients[0].BuyPrice = clients[0].Cost * clients[0].Volume;
                clients[0].SellPrice = clients[0].Cost * clients[0].Volume;
                clients[0].NetVolume = (SELL ? 0 : clients[0].Volume);
                clients[0].SumOfNetPrice = (SELL ? 0 : clients[0].BuyPrice);


            }
            else
            {
                for (int i = 1; i < clients.Count; i++)
                {
                    SELL = clients[i].Side == "SELL";
                    clients[i].BuyPrice = clients[i].Cost * clients[i].Volume;
                    clients[i].SellPrice = clients[i].Cost * clients[i].Volume;
                    clients[i].NetVolume = (SELL ? clients[i - 1].NetVolume - clients[i].Volume : clients[i - 1].NetVolume + clients[i].Volume);
                    Console.WriteLine("side:{0}, Prev NETV:{1}, NETV:{2}", clients[i].Side, clients[i - 1].NetVolume, clients[i].NetVolume);
                    clients[i].SumOfNetPrice = (SELL ? clients[i - 1].AverageCost * clients[i].NetVolume : clients[i - 1].SumOfNetPrice + clients[i].BuyPrice);
                    clients[i].AverageCost = (clients[i].NetVolume.Value == 0) ? 0m : clients[i].SumOfNetPrice.Value / clients[i].NetVolume.Value;
                    Console.WriteLine("side:{0}, Prev AvgCost:{1}, NETV:{2}, NetPrice: {3}", clients[i].Side, clients[i - 1].AverageCost, clients[i].NetVolume, clients[i].SumOfNetPrice);

                }

            }


            //netV = Convert.ToInt16(oneRecordOnly) * (SELL ? 0 : clients[0].Volume)
            //    + Convert.ToInt16(twoRecords) * (SELL ? 0 : clients[1].Volume);

            //clients[0].NetVolume = netV;

            //first operand is 
            //var netP = Convert.ToInt16(oneRecordOnly) * (SELL ? 0 : clients[0].BuyPrice)
            //    + Convert.ToInt16(twoRecords) * (SELL ? clients[0].AverageCost * clients[1].NetVolume : clients[0].BuyPrice + clients[1].BuyPrice);

            //clients[1].SumOfNetPrice = netP;

            foreach (ClientAverageCost client in clients)
            {
                //client.SumOfNetPrice = netP;
                client.AverageCost = (client.NetVolume.Value == 0) ? 0m : client.SumOfNetPrice.Value / client.NetVolume.Value;
            }
            //client[0].AverageCost = (client[0].NetVolume.Value == 0) ? 0m : client[0].SumOfNetPrice.Value / client[0].NetVolume.Value;
            //client[1].AverageCost = 
            int buy = Convert.ToInt16(BUY);
            int sell = Convert.ToInt16(SELL);

            //x and y = formula
            int x = 1;
            int y = 1;

            //avgCost = buy * (x) + sell*(y);

            //return client;
        }
    }
}
