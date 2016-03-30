using COMClassLibrary.Manager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace PortfolioController
{
    public class StockPriceManager
    {


        public static Dictionary<string, decimal> GetStockClosingPrice(List<string> stocks)
        {
            Dictionary<string, decimal> stockPrices = new Dictionary<string, decimal>();
            StockParser s = new StockParser();
            foreach (string stock in stocks)
            {
                var quotes = Request(stock);
                stockPrices.Add(stock, Convert.ToDecimal(quotes.close));
            }        
            
            return stockPrices;
        }

        public static Security Request(string q)
        {

            //Console.WriteLine(response);
            try
            {
                string url = ConfigurationManager.AppSettings["url"].ToString();
                string stockInfoPage = ConfigurationManager.AppSettings["stockInfoPage"].ToString();
                string theUrlGet = String.Format("{0}/quotes/{1}?code=", url, stockInfoPage);

                string code = q;
                Security sec = new Security();
                if (code != String.Empty)
                {
                    WebClient wb = new WebClient();
                    byte[] arr = wb.DownloadData(theUrlGet + code);

                    string response = Encoding.ASCII.GetString(arr);
                    StockParser parser = new StockParser();
                    sec = parser.ParseStockXML(response);

                    wb.Dispose();

                }
                return sec;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message
                    );
                return null;
            }
        }
        public Security ParseStockXML(String xml)
        {
            //String lastPrice = string.Empty;
            //string open = string.Empty;
            //string high = string.Empty;
            //string low = string.Empty;
            //String close = string.Empty; 
            //String volume = string.Empty;

            Security sec = new Security();

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/tstock/security");
            //ListBoo
            foreach (XmlNode node in nodes)
            {
                XmlNode stockInfoNode = node.SelectSingleNode("stockinfo");
                if (stockInfoNode == null) break;
                sec.lastPrice = stockInfoNode.SelectSingleNode("last").InnerText;
                sec.close = stockInfoNode.SelectSingleNode("prevclose").InnerText;
                sec.open = stockInfoNode.SelectSingleNode("open").InnerText;
                sec.high = stockInfoNode.SelectSingleNode("high").InnerText;
                sec.low = stockInfoNode.SelectSingleNode("low").InnerText;
                sec.volume = stockInfoNode.SelectSingleNode("volume").InnerText;
                sec.status = stockInfoNode.SelectSingleNode("status").InnerText;
                XmlNode bidNode = stockInfoNode.SelectSingleNode("bid");
                XmlNodeList bidVolNodes = bidNode.SelectNodes("vol");
                XmlNodeList bidPriceNodes = bidNode.SelectNodes("price");

                XmlNode askNode = stockInfoNode.SelectSingleNode("ask");
                XmlNodeList askVolNodes = askNode.SelectNodes("vol");
                XmlNodeList askPriceNodes = askNode.SelectNodes("price");

                decimal price;
                int volume;
                int numberOfBids = 1;
                int numberOfAsks = 1;

                for (int i = 0; i < bidPriceNodes.Count; i++)
                {
                    Decimal.TryParse(bidPriceNodes[i].InnerText, out price);
                    int.TryParse(bidVolNodes[i].InnerText.Replace(",", ""), out volume);

                    sec.Bids.Add(
                        new BidAsk((i + 1).ToString()
                            , "Bid", price, volume, numberOfBids)
                        );
                }

                for (int i = 0; i < askPriceNodes.Count; i++)
                {
                    Decimal.TryParse(askPriceNodes[i].InnerText, out price);
                    int.TryParse(askVolNodes[i].InnerText.Replace(",", ""), out volume);

                    sec.Asks.Add(
                        new BidAsk((i + 1).ToString()
                            , "Ask", price, volume, numberOfAsks)
                        );
                }


            }
            return sec;
        }
    }
}
