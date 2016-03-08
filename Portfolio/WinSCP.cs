using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace Portfolio
{
    public class WinSCP
    {
        public static void uploadFile(string csvPath)
        {
            
            try
            {
                // Setup session options
                SessionOptions sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = ConfigurationManager.AppSettings["host"].ToString(),
                    UserName = ConfigurationManager.AppSettings["username"].ToString(),
                    Password = ConfigurationManager.AppSettings["password"].ToString(),
                    SshHostKeyFingerprint = ConfigurationManager.AppSettings["sshFingerprint"].ToString()
                };

                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);
                    Console.WriteLine("WinSCP connection success..");
                    // Upload files
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Automatic;
                   
                    TransferOperationResult transferResult;
                    transferResult = session.PutFiles(csvPath, "/", false, transferOptions);
                    Console.WriteLine("upload to path: /");
                    // Throw on any error
                    transferResult.Check();
                    
                    // Print results
                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        Console.WriteLine("Upload of {0} succeeded", transfer.FileName);
                    }
                }

               
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e);
                
            }
        }
    }
}
