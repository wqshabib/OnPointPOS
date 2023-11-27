using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using POSSUM.Utils;

namespace POSSUM.SyncService
{
    public partial class POSSUMSyncService : ServiceBase
    {
        #region properties/variables      
        Timer timer = null;
        private int interval = 1;
        private string terminalId = "";
        private int TerminalMode = 0;
        private string StockFileBasePath = "";


        SyncController syncController = null;
        #endregion
        public POSSUMSyncService()
        {
            InitializeComponent();
            interval = Convert.ToInt32(ConfigurationManager.AppSettings["Interval"]);
            terminalId = ConfigurationManager.AppSettings["TerminalId"];
            int terminalMode = 0;
            int.TryParse(ConfigurationManager.AppSettings["TerminalMode"], out terminalMode);
            TerminalMode = terminalMode;
            StockFileBasePath = ConfigurationManager.AppSettings["StockFileBasePath"];

        }
        public void OnDebug()
        {
            syncController = new SyncController();
            syncController.ValidateData();
            //OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            timer = new Timer();
            timer.Interval = (1000 * 60) * interval;
            timer.Start();
            timer.Elapsed += Timer_Elapsed; ;
            timer.AutoReset = false;
            Log.WriteLog("Service started...");

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Log.WriteLog("Timer started...");

            timer.Enabled = false;
            PerformTask();
            timer.Enabled = true;
            GC.Collect();

        }
        private void PerformTask()
        {
            try
            {
                Log.WriteLog("Perform Task started...");

                string localConnectionString = ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;
                string _baseAddress = ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"];
                if (TerminalMode == 0)
                {

                    string apiUser = ConfigurationManager.AppSettings["ApiUserID"];
                    string apiPwd = ConfigurationManager.AppSettings["ApiPassword"];
                    StartTask(localConnectionString, _baseAddress, terminalId, "log", apiUser, apiPwd);
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                }
                else
                {
                    string UserID1 = ConfigurationManager.AppSettings["UserID"];
                    string UserID2 = ConfigurationManager.AppSettings["UserID2"];
                    var settings1 = ReadLocalSettings(UserID1);
                    var settings2 = ReadLocalSettings(UserID2);
                    StartTask(settings1.ConnectionString, settings1.SyncAPIUri, settings1.TerminalId, "Outlet1log", settings1.ApiUserID, settings1.ApiPassword);
                    StartTask(settings2.ConnectionString, settings2.SyncAPIUri, settings2.TerminalId, "Outlet2log", settings2.ApiUserID, settings2.ApiPassword);
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
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
        }
        private void StartTask(string localConnectionString, string _baseAddress, string terminalId, string log, string apiUser, string apiPassword)
        {
            Log.WriteLog("Data Sync started...", log);
            syncController = new SyncController();

            var performStockAdjustment = ConfigurationManager.AppSettings["StockAdjustmentEnabled"];
            if (performStockAdjustment == "1")
            {
                Log.WriteLog("Downloading FTP File for stock...", log);
                syncController.DownloadFile(DateTime.Now, Guid.Parse(terminalId), localConnectionString, _baseAddress, apiUser, apiPassword);
                Log.WriteLog("Updating stock from sale...", log);
                syncController.UpdateStockAndWeight(localConnectionString);
            }

            bool res = syncController.DataSync(DateTime.Now, Guid.Parse(terminalId), localConnectionString, _baseAddress, apiUser, apiPassword);
            if (res)
                Log.WriteLog("Data Sync completed...", log);
            syncController = null;
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);


        private void RestartService()
        {
            try
            {
                System.Diagnostics.ProcessStartInfo startInfo =
      new System.Diagnostics.ProcessStartInfo();
                startInfo.FileName = "POSSUM.SyncServiceStarter.exe";
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                Process.Start(startInfo);
            }
            catch
            {


            }
        }

        protected override void OnStop()
        {
            Log.WriteLog("Service Stopped...");
        }
        public static LocalSetting ReadLocalSettings(string userName)
        {
            try
            {
                string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string settingspath = @rootpath + @"\settings.xml";
                XElement main = XElement.Load(settingspath);

                var results = main.Descendants("Terminal")
                        .Where(e => e.Attribute("UserName").Value == userName)
                    .Select(e => new LocalSetting
                    {
                        ConnectionString = e.Descendants("ConnectionString").FirstOrDefault().Value,
                        TerminalId = e.Descendants("TerminalId").FirstOrDefault().Value,
                        SyncAPIUri = e.Descendants("SyncAPIUri").FirstOrDefault().Value,
                        ApiUserID = e.Descendants("ApiUserID").FirstOrDefault().Value,
                        ApiPassword = e.Descendants("ApiPassword").FirstOrDefault().Value
                    });

                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Log.WriteLog("Error in Xml Settings Reading " + ex.ToString());
                return null;
            }
        }
    }
    public class LocalSetting
    {
        public string ConnectionString { get; set; }
        public string TerminalId { get; set; }
        public string SyncAPIUri { get; set; }
        public string PaymentDeviceType { get; set; }
        public string PaymentDevicConnectionString { get; set; }

        public string ApiUserID { get; set; }
        public string ApiPassword { get; set; }

    }
}

