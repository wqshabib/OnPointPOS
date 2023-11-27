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

namespace POSSUM.Integration.Services
{
    public partial class POSSUMIntegrationServices : ServiceBase
    {
        #region properties/variables      
        Timer timer = null;
        private int interval = 1;
        private string terminalId = "";
        private int TerminalMode = 0;
        private string StockFileBasePath = "";
        FortNox _fortNox;

        SyncController syncController = null;
        #endregion
        public POSSUMIntegrationServices()
        {
            InitializeComponent();
            interval = Convert.ToInt32(ConfigurationManager.AppSettings["Interval"]);
            //terminalId = ConfigurationManager.AppSettings["TerminalId"];
            //int terminalMode = 0;
            //int.TryParse(ConfigurationManager.AppSettings["TerminalMode"], out terminalMode);
            //TerminalMode = terminalMode;
            //StockFileBasePath = ConfigurationManager.AppSettings["StockFileBasePath"];

        }
        public void OnDebug()
        {
            PerformTask();
            //syncController = new SyncController();
            //syncController.ValidateData();
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

                //string localConnectionString = ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;
                //string _baseAddress = ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"];
                string dbName = ConfigurationManager.AppSettings["FNDBName"];

                if (_fortNox == null)
                    _fortNox = new FortNox(dbName);

                if (!_fortNox.IsWorking)
                {
                    _fortNox.Sync();
                }

                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
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

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);


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

