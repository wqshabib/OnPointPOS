using System;
using System.IO;
using System.Reflection;
using POSSUM.Model;
using System.ComponentModel;
using POSSUM.Data;
using System.Text;
using POSSUM.Utils;
using System.Configuration;

namespace POSSUM.StandAloneApi
{
    public class LogWriter
    {
        private static string m_exePath = string.Empty;
        private static string rootpath = ConfigurationManager.AppSettings["LogsRootPath"];
        static string CurrentException = string.Empty;

        public void CashGaurdLog(string logMessage)
        {

            try
            {
                string logfile = "log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";
                string dirPath = rootpath + @"\logs\CashGaurd";
                string path = dirPath + logfile;

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

        public static void LogWrite(string logMessage)
        {
            string pathroot = rootpath;
            m_exePath = pathroot;

            try
            {
                DirCheck(m_exePath + "\\Logs");

                using (var w = File.AppendText(m_exePath + "\\Logs\\" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + "_Log_Version7.txt"))
                {
                    Log(logMessage, w);
                }
            }
            catch (Exception ex)
            {
                LogWrite(ex);
            }
        }


        public static void LogWrite(Exception exp)
        {
            try
            {
                LogException(exp);
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
            SendEmail(CurrentException);
        }

        public static void LogException(Exception exc)
        {
            // Include enterprise logic for logging exceptions 
            // Get the absolute path to the log file 
            string logfile = "Exception_log_version7_" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";
            var pathroot = rootpath;
            string dirPath = pathroot + @"\logs\";
            string path = dirPath + logfile;

            if (!File.Exists(path))
            {
                DirCheck(dirPath);
            }

            // Create a file to write to. 
            using (StreamWriter sw = File.AppendText(path))
            {

                sw.WriteLine("********** {0} **********", DateTime.Now);
                sw.WriteLine("{0}", exc.ToString());
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


        public static void SendEmail(string message)
        {
            try
            {
                var cc = ConfigurationManager.AppSettings["MailAddressCC"];

                string receiver = "zahid@deltronsystems.com,khaliljandran@gmail.com";// Receiver email
                string subject = "Api exception";// Defaults.Outlet.Name + " Exception";

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("<div style='padding: 10px 30px 0'>");
                stringBuilder.AppendLine("<p style ='margin: 0 0 10px; padding: 0; font - size:16px; color:#111111'> Hello, " + "POSSUM" + "</ p >");
                stringBuilder.AppendLine("<div style = 'width: 100 %; height: 1px; background:#cfcfcf; margin-top:25px; max-height:1px' ></ div >");
                stringBuilder.AppendLine("<hr/>");
                stringBuilder.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111' > Exception on " + subject + "</ p >");
                stringBuilder.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111;' > " + message + "</ p >");
                stringBuilder.AppendLine("</div>");

                new SMTPClient().SendMail(receiver, cc, subject, message, stringBuilder.ToString());
            }
            catch (Exception ex)
            {
                //
            }
        }

        public void LogDibsLogs(string logMessage)
        {

            try
            {
                string logfile = "log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";
                string dirPath = rootpath + @"\logs\DibsLogs\";
                string path = dirPath + logfile;

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
    }
}