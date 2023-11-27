using System;
using System.IO;
using System.Reflection;
using POSSUM.Model;
using System.ComponentModel;
using POSSUM.Data;
using System.Text;
using System.Threading;


namespace POSSUM.Integration
{
    public class IntegrationLogWriter
    {
        private static string m_exePath = string.Empty;
        static string CurrentException = string.Empty;
        private static object _obj = new Object();
        private static ReaderWriterLockSlim _readWriteLock = new ReaderWriterLockSlim();

        public void CashGaurdLog(string logMessage)
        {

            try
            {
                string logfile = "log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";

                string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string dirPath = rootpath + @"\logs\CashGaurd";
                string path = dirPath + logfile;
                // This text is added only once to the file. 

                if (!File.Exists(path))
                {
                    DirCheck(dirPath);

                }

                using (StreamWriter w = File.AppendText(path))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.IntegrationLogWrite("CashGaurdLog >> " + ex);
            }
        }

        public static void CleanCashJsonLogWrite(string filename, string logMessage)
        {

            try
            {
                string logfile = "CleanCash_" + filename + ".json";

                string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string dirPath = rootpath + @"\CleanCashLogs\";
                string path = dirPath + logfile;
                // This text is added only once to the file. 

                if (!File.Exists(path))
                {
                    DirCheck(dirPath);

                }

                using (StreamWriter w = File.AppendText(path))
                {
                    SaveCleanCashLog(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.IntegrationLogWrite("CleanCashJsonLogWrite >> " + ex);
            }
        }
        public static void CleanCashLogWrite(string logMessage)
        {
            try
            {
                logMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "- " + logMessage;
                string logfile = "Log_" + +DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt";

                string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string dirPath = rootpath + @"\CleanCashLogs\";
                string path = dirPath + logfile;
                // This text is added only once to the file. 

                if (!File.Exists(path))
                {
                    DirCheck(dirPath);

                }

                using (StreamWriter w = File.AppendText(path))
                {
                    SaveCleanCashLog(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.IntegrationLogWrite("CleanCashLogWrite >> " + ex);
            }
        }
        public static void CloudCleanCashLogWrite(string logMessage)
        {
            try
            {
                logMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "- " + logMessage;
                string logfile = "Log_" + +DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt";

                string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string dirPath = rootpath + @"\CloudCleanCashLogs\";
                string path = dirPath + logfile;
                // This text is added only once to the file. 

                if (!File.Exists(path))
                {
                    DirCheck(dirPath);

                }

                using (StreamWriter w = File.AppendText(path))
                {
                    SaveCleanCashLog(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.IntegrationLogWrite("CleanCashLogWrite >> " + ex);
            }
        }
        private static void SaveCleanCashLog(string logMessage, TextWriter txtWriter)
        {
            try
            {
                txtWriter.WriteLine(logMessage);
            }
            catch (Exception ex)
            {

                Console.WriteLine(logMessage);

            }
        }

        public static void IntegrationLogWrite(string logMessage)
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                DirCheck(m_exePath + "\\Logs");

                using (
                    var w =
                        File.AppendText(m_exePath + "\\Logs\\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day +
                                        "IntegrationLogWriter_LogWrite.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.IntegrationLogWrite("LogWrite >> " + ex);
            }
        }       

        public static void BabsLogWrite(string logMessage)
        {
            lock (_obj)
            {
                m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                try
                {

                    DirCheck(m_exePath + "\\Logs");

                    using (
                        var w =
                            File.AppendText(m_exePath + "\\Logs\\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day +
                                            "_BabsLog.txt"))
                    {
                        Log(logMessage, w);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationLogWriter.IntegrationLogWrite("BabsLogWrite >> " + ex);
                }
            }
        }

       
        public static void LogWrite(Exception exp)
        {
            try
            {

                LogException(exp);

                //SaveLog(exp.ToString(), "log");

            }
            catch (Exception ex)
            { 
                IntegrationLogWriter.IntegrationLogWrite("LogWrite >> " + ex);             
            }
        }             

        
        public static void LogWrite(string logMessage, string folderName = "IntegrationLogs")
        {
            lock (_obj)
            {

                try
                {
                    string logfile = "log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";

                    string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    string dirPath = rootpath + @"\" + folderName + @"\";
                    string path = dirPath + logfile;
                    // This text is added only once to the file. 

                    if (!File.Exists(path))
                    {
                        DirCheck(dirPath);

                    }
                    _readWriteLock.EnterReadLock();

                    using (StreamWriter w = File.AppendText(path))
                    {
                        Log(logMessage, w);
                    }
                }
                catch (Exception ex)
                {
                    IntegrationLogWriter.LogWrite(ex);
                }
                finally
                {
                    _readWriteLock.ExitReadLock();
                }
            }
        }

        private static void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            SendEmail(CurrentException);
        }

        public static void LogException(Exception exc)
        {
            // Include enterprise logic for logging exceptions 
            // Get the absolute path to the log file 
            string logfile = "IntegrationLogWriter_Log_" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";

            string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string dirPath = rootpath + @"\logs\";
            string path = dirPath + logfile;
            // This text is added only once to the file. 

            CurrentException = exc.ToString();
            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += Bg_DoWork;
            bg.RunWorkerAsync();

            if (!File.Exists(path))
            {
                DirCheck(dirPath);
            }

            // Create a file to write to. 
            using (StreamWriter sw = File.CreateText(path))
            {

                sw.WriteLine("********** {0} **********", DateTime.Now);
                if (exc.InnerException != null)
                {
                    sw.Write("Inner Exception Type: ");
                    sw.WriteLine(exc.InnerException.GetType().ToString());
                    sw.Write("Inner Exception: ");
                    sw.WriteLine(exc.InnerException.Message);
                    sw.Write("Inner Source: ");
                    sw.WriteLine(exc.InnerException.Source);
                    if (exc.InnerException.StackTrace != null)
                    {
                        sw.WriteLine("Inner Stack Trace: ");
                        sw.WriteLine(exc.InnerException.StackTrace);
                    }
                }
                sw.Write("Exception Type: ");
                sw.WriteLine(exc.GetType().ToString());
                sw.WriteLine("Exception: " + exc.Message);

                sw.WriteLine("Stack Trace: ");
                if (exc.StackTrace != null)
                {
                    sw.WriteLine(exc.StackTrace);
                    sw.WriteLine();
                }
            }
        }

        private static void Log(string logMessage, TextWriter txtWriter)
        {
            try
            {
#if DEBUG
                Console.WriteLine(logMessage);
#endif
                txtWriter.Write("\r\nLog Entry : ");
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(logMessage);
#endif
            }
        }

        private static void Log(Exception logMessage, TextWriter txtWriter)
        {
            try
            {
#if DEBUG
                Console.WriteLine(logMessage);
#endif
                txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString());
                txtWriter.WriteLine("  :");
                txtWriter.WriteLine("  :{0}", logMessage.Message);
                txtWriter.WriteLine("  :{0}", logMessage.StackTrace);
                txtWriter.WriteLine("-------------------------------");
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(logMessage);
#endif
            }
        }


        

        public static void PerformanceLog(string text)
        {
            SaveLog(text, "Performance");
        }

        private static void SaveLog(string message, string filePrefix)
        {
            IntegrationLogWriter.IntegrationLogWrite(message);
        }

        private static void DirCheck(string path)
        {
            if (Directory.Exists(path)) return;
            var items = path.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            string _path = "";
            foreach (var item in items)
            {
                _path = _path + item + "\\";
                if (Directory.Exists(_path) == false)
                    Directory.CreateDirectory(_path);
            }
        }

        #region JOURNAL LOG

        public static void JournalLog(int actionid)
        {
            JournalLog(actionid, null);
        }

        public static void JournalLog(int actionid, Guid? orderId)
        {
            JournalLog(actionid, orderId, null, null);
        }

        public static void JournalLog(JournalActionCode code, Guid? orderId)
        {
            JournalLog(Convert.ToInt16(code), orderId);
        }

        private static Journal _journal = null;
        public static void JournalLog(int actionid, Guid? orderId, Guid? itemId, int? tableId, string logMessage = "")
        {
            var journal = new Journal
            {
                OrderId = orderId,
                ItemId = itemId,
                TableId = tableId,
                ActionId = actionid,
                LogMessage = logMessage,
                Created = DateTime.Now
            };
            if (DefaultsIntegration.User != null && !string.IsNullOrEmpty(DefaultsIntegration.User.Id))
                journal.UserId = DefaultsIntegration.User.Id;
            _journal = journal;
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerAsync();
        }

        private static void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {


                    db.Journal.Add(_journal);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.IntegrationLogWrite("BackgroundWorker_DoWork >>> " + ex);
            }
        }

        public static void JournalLog(JournalActionCode code)
        {
            JournalLog(Convert.ToInt16(code));
        }

        internal static void JournalLog(JournalActionCode code, Guid orderId, Guid itmid)
        {
            short actionId = Convert.ToInt16(code);

            JournalLog(actionId, orderId, itmid, null);
        }


        #endregion
        public void CashGaurdEventLog(string fileName, string logMessage)
        {

            try
            {
                string logfile = fileName + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";

                string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string dirPath = rootpath + @"\logs\CashGaurd\";
                string path = dirPath + logfile;
                // This text is added only once to the file. 

                if (!File.Exists(path))
                {
                    DirCheck(dirPath);

                }

                using (StreamWriter w = File.AppendText(path))
                {
                    Log(logMessage, w);
                }
            }
            catch
            {
                //LogWriter.LogWrite(ex);
            }
        }
        public static void DeviceLog(Guid orderId, decimal orderTotal, decimal vatAmount, decimal cashBack)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {

                    var deviceLog = new PaymentDeviceLog
                    {
                        OrderId = orderId,
                        OrderTotal = orderTotal,
                        VatAmount = vatAmount,
                        CashBack = cashBack,
                        SendDate = DateTime.Now,
                        Synced = false
                    };

                    db.PaymentDeviceLog.Add(deviceLog);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                IntegrationLogWriter.IntegrationLogWrite("DeviceLog >> " + ex);
            }
        }

        public static void SendEmail(string message)
        {
            try
            {

                //string receiver = "zahid@deltronsystems.com";// Receiver email
                //string subject = DefaultsIntegration.Outlet.Name + " Exception";


                //var client = new SMTPClient();

                //string html = "";

                //StringBuilder str = new StringBuilder();
                //str.AppendLine("<div style='padding: 10px 30px 0'>");
                //str.AppendLine("<p style ='margin: 0 0 10px; padding: 0; font - size:16px; color:#111111'> Hello, " + "POSSUM" + "</ p >");

                //str.AppendLine("<div style = 'width: 100 %; height: 1px; background:#cfcfcf; margin-top:25px; max-height:1px' ></ div >");
                //str.AppendLine("<hr/>");
                //str.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111' > Exception on " + Defaults.Terminal.UniqueIdentification + "</ p >");

                //str.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111;' > " + message + "</ p >");

                //str.AppendLine("</div>");

                //html = str.ToString();
                //client.SendMail(receiver, subject, message, html);
            }
            catch (Exception ex)
            {
                //
            }


        }

       
    }

}