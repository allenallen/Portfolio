using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class Dealers
    {
        private List<PortfolioModel> d1;

        public List<PortfolioModel> D1
        {
            get { return d1; }
            set { d1 = value; }
        }
        private List<PortfolioModel> dasys;

        public List<PortfolioModel> Dasys
        {
            get { return dasys; }
            set { dasys = value; }
        }
        private List<PortfolioModel> d4;

        public List<PortfolioModel> D4
        {
            get { return d4; }
            set { d4 = value; }
        }
        private List<PortfolioModel> mm;

        public List<PortfolioModel> Mm
        {
            get { return mm; }
            set { mm = value; }
        }

        private Dictionary<string, double> currency;

        public Dictionary<string, double> Currency
        {
            get { return currency; }
            set { currency = value; }
        }
    }
}
