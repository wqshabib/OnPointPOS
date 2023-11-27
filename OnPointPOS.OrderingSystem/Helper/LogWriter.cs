using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using static System.Net.Mime.MediaTypeNames;

namespace POSSUM
{
    public class LogWriter
    {
        private static string m_exePath = string.Empty;
        public void CashGaurdLog(string logMessage)
        {

            try
            {
                string logfile = "log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";
                var div = Directory.GetCurrentDirectory();
                var rootpath = @"C:\Sites\MOFR\onlineorderingsystem.mofr.se"; 
                //string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string dirPath = rootpath + @"\logs\WChat";
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
                LogWriter.LogWrite(ex);
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
                LogWriter.LogWrite(ex);
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
                LogWriter.LogWrite(ex);
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

        public static void LogWrite(string logMessage)
        {
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                DirCheck(m_exePath + "\\Logs");

                using (
                    var w =
                        File.AppendText(m_exePath + "\\Logs\\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day +
                                        "_Log.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
        }

        static string CurrentException = string.Empty;

        public static void LogWrite(Exception exp)
        {
            try
            {

                LogException(exp);



                //  SaveLog(exp.ToString(), "log");

            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.ToString());
#endif
            }
        }

        private static void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            //SendEmail(CurrentException);
        }

        public static void LogException(Exception exc)
        {
            // Include enterprise logic for logging exceptions 
            // Get the absolute path to the log file 
            string logfile = "log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";

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


        //public static void CheckOutLogWrite(string logMessage, Guid orderId)
        //{
        //    if (Defaults.EnableCheckoutLog == false)
        //        return;
        //    try
        //    {
        //        logMessage = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "- " + logMessage;
        //        string logfile = orderId + "_Log_" + +DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt";

        //        string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        //        string dirPath = rootpath + @"\Logs\";
        //        string path = dirPath + logfile;
        //        // This text is added only once to the file. 

        //        if (!File.Exists(path))
        //        {
        //            DirCheck(dirPath);

        //        }

        //        using (StreamWriter w = File.AppendText(path))
        //        {
        //            SaveCleanCashLog(logMessage, w);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWriter.LogWrite(ex);
        //    }
        //}

        public static void PerformanceLog(string text)
        {
            SaveLog(text, "Performance");
        }

        private static void SaveLog(string message, string filePrefix)
        {
            LogWriter.LogWrite(message);
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

       
        //public void CashGaurdEventLog(string fileName, string logMessage)
        //{

        //    try
        //    {
        //        string logfile = fileName + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";

        //        string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        //        string dirPath = rootpath + @"\logs\CashGaurd\";
        //        string path = dirPath + logfile;
        //        // This text is added only once to the file. 

        //        if (!File.Exists(path))
        //        {
        //            DirCheck(dirPath);

        //        }

        //        using (StreamWriter w = File.AppendText(path))
        //        {
        //            Log(logMessage, w);
        //        }
        //    }
        //    catch
        //    {
        //        //LogWriter.LogWrite(ex);
        //    }
        //}
        //public static void DeviceLog(Guid orderId, decimal orderTotal, decimal vatAmount, decimal cashBack)
        //{
        //    try
        //    {
        //        using (var db = new ApplicationDbContext())
        //        {

        //            var deviceLog = new PaymentDeviceLog
        //            {
        //                OrderId = orderId,
        //                OrderTotal = orderTotal,
        //                VatAmount = vatAmount,
        //                CashBack = cashBack,
        //                SendDate = DateTime.Now,
        //                Synced = false
        //            };

        //            db.PaymentDeviceLog.Add(deviceLog);
        //            db.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogWrite(ex);
        //    }
        //}

        //public static void SendEmail(string message)
        //{
        //    try
        //    {

        //        string receiver = "zahid@deltronsystems.com";// Receiver email
        //        string subject = Defaults.Outlet.Name + " Exception";


        //        var client = new SMTPClient();

        //        string html = "";

        //        StringBuilder str = new StringBuilder();
        //        str.AppendLine("<div style='padding: 10px 30px 0'>");
        //        str.AppendLine("<p style ='margin: 0 0 10px; padding: 0; font - size:16px; color:#111111'> Hello, " + "POSSUM" + "</ p >");

        //        str.AppendLine("<div style = 'width: 100 %; height: 1px; background:#cfcfcf; margin-top:25px; max-height:1px' ></ div >");
        //        str.AppendLine("<hr/>");
        //        str.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111' > Exception on " + Defaults.Terminal.UniqueIdentification + "</ p >");

        //        str.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111;' > " + message + "</ p >");

        //        str.AppendLine("</div>");

        //        html = str.ToString();
        //        client.SendMail(receiver, subject, message, html);
        //    }
        //    catch (Exception ex)
        //    {
        //        //
        //    }


        //}


    }

}