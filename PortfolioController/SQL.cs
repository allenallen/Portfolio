using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioController
{
    public class SQL
    {
        public static void GetReport1()
        {
            string connectionString = "Data Source=10.11.18.21;User ID=sa;"
                                               + "Password=password-2014;";

            using (SqlConnection connection =
            new SqlConnection(connectionString))
            {

                string qString = File.ReadAllText("");
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(qString, connection);
                Console.WriteLine("Connection succeeded");
                //command.Parameters.AddWithValue("@pricePoint", paramValue);

                // Open the connection in a try/catch block. 
                // Create and execute the DataReader, writing the result
                // set to the console window.
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader != null)
                        Console.WriteLine("not null");
                    while (reader.Read())
                    {
                        int fieldCount = reader.FieldCount;
                        for (int i = 0; i < fieldCount; i++)
                        {
                            Console.WriteLine(reader.GetName(i) + ": " + reader[i].ToString());
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                Summary s = null;
                //AverageCostCalculator ac = new AverageCostCalculator("BUY",s);
                //Console.WriteLine(ac.AvgCost);

                //sdb.Test();

                Console.ReadLine();
            }
        }
    }
}
