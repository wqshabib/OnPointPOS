using Newtonsoft.Json;
using POSSUM.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace POSSUM.SyncData
{
    public partial class PossumSyncData : ServiceBase
    {
        Timer timer = null;
        StockRepository stockRepository = new StockRepository();

        private int interval = Convert.ToInt32(ConfigurationManager.AppSettings["Interval"]);
        private string filePath = ConfigurationManager.AppSettings["StockFilePath"];
        public PossumSyncData()
        {
            InitializeComponent();
            //interval = interval;  //1 minutes
        }

        public void OnDebug()
        {
            Log.WriteLog("Service started OnDebug...");
            //OnStart(null);
            //stockRepository.UpdateStock(@"D:\Luqon-IT\POSSUM\test.csv");
            //PerformTask();
        }
        protected override void OnStart(string[] args)
        {
            Log.WriteLog("Service started...");
            timer = new Timer();
            timer.Interval = (1000 * 60) * interval;
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = false;

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            PerformTask();
            timer.Enabled = true;
            GC.Collect();

        }
        private void PerformTask()
        {
            try
            {
                Log.WriteLog("PerformTask  started...");

                var lastExecutedTime = Log.ReadLastExecutedDateTimeFromFile();
                var directory = new DirectoryInfo(filePath);
                var newFile = directory.GetFiles().OrderByDescending(f => f.CreationTime).First();

                if (newFile != null && newFile.CreationTime >= Convert.ToDateTime(lastExecutedTime))
                {
                    Log.WriteLog("File found: " + newFile.Name);
                    Log.WriteLog("UpdateStock started.....");
                    //stockRepository.UpdateStock(filePath + newFile.Name);
                    Log.WriteLog("UpdateStock Completed.....");

                }
            }

            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
            }
            finally
            {
                GC.Collect();
            }
            Log.WriteLastExecutedDateTimeInFile(DateTime.Now.ToString());
            Log.WriteLog("PerformTask  Completed...");


        }


        protected override void OnStop()
        {
            Log.WriteLog("Service Stopped...");
        }


    }


}

