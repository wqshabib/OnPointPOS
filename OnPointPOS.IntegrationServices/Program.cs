using POSSUM;
using POSSUM.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration.Services
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
       
        static void Main()
        {
#if DEBUG
            POSSUMIntegrationServices syncUtility = new POSSUMIntegrationServices();
            syncUtility.OnDebug();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
                                                { 
                                                    new POSSUMIntegrationServices() 
                                                };
            ServiceBase.Run(ServicesToRun);
#endif

        }


    }
}
