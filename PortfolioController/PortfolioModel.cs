using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class PortfolioModel
    {
         private string portfolioName;

        public string PortfolioName
        {
            get { return portfolioName; }
            set { portfolioName = value; }
        }
        private string date;

        public string Date
        {
            get { return date; }
            set { date = value; }
        }
        private string securityID;

        public string SecurityID
        {
            get { 
                return securityID; 
            }
            set { securityID = value; }
        }
        private string position;

        public string Position
        {
            get { return position; }
            set { position = value; }
        }
        private string avgCost;

        public string AvgCost
        {
            get { return avgCost; }
            set { avgCost = value; }
        }

        public PortfolioModel() { }

        public PortfolioModel(string portfolioName, string date, string securityId, string position, string avgCost)
        {
            this.PortfolioName = portfolioName;
            this.Date = date;
            this.SecurityID = securityId;
            this.Position = position;
            this.AvgCost = avgCost;
        }

        
    }
}
