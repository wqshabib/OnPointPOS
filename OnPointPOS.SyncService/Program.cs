using POSSUM;
using POSSUM.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.SyncService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
       
        static void Main()
        {
#if DEBUG
            var myController = new POSSUMSyncService();
            myController.OnDebug();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);


            var syncController = new SyncController();
            //syncController.GetProducts();

            Guid terminalId = Guid.Parse(ConfigurationManager.AppSettings["TerminalId"]);// Guid.Parse("613C5E1C-460F-4F6B-AD87-EA376066BDAE");
            string localConnectionString = ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;
            string _baseAddress = ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"];
            string apiUser = ConfigurationManager.AppSettings["ApiUserID"];
            string apiPassword = ConfigurationManager.AppSettings["ApiPassword"];
            bool res = syncController.DataSync(DateTime.Now, terminalId, localConnectionString, _baseAddress, apiUser, apiPassword);
            // //syncController.DownloadFile(DateTime.Now, terminalId, localConnectionString, _baseAddress, apiUser, apiPassword);



#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
                                                { 
                                                    new POSSUMSyncService() 
                                                };
            ServiceBase.Run(ServicesToRun);
#endif

        }
        

    }
}
