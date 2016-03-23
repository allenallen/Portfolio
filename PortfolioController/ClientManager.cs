using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class ClientManager
    {
        public static List<Client> GetAllClients()
        {
            List<Client> list = null;
            using (var dbContext = new StockXDataBaseEntities())
            {
                list = (from match in dbContext.Client
                                       select match).ToList<Client>();
            }
            return list;
        }
        public static void GenerateClients(List<Client> list)
        {

            CsvWrite.WriteClient(list);
            //OT9521-7;MARCO N. ACCAD;Unit 406 Danube Bldg., Riverfront Residences Caniogan, Pasig 1606 ;(0917)862-8504 631-8734;0.0025;N;C;5;11011205464539
            //foreach (var item in list)
            //{
            //    Console.WriteLine("name:{0}", item.Name);
            //}
        }
    }
}
