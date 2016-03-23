using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class PledgeManager
    {
        public static void GeneratePledge(List<Client> list)
        {

            CsvWrite.WritePledge(list);
        }

    }
}
