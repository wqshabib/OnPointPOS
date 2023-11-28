using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class POSSUMDataLog
    {


        #region Save Log
        private static Object thisLock = new Object();
        public static void WriteLog(string message, string log = "log")
        {
            SaveLog2(message, log);
        }
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
        public static void LogException(Exception exc)
        {
            // Include enterprise logic for logging exceptions 
            // Get the absolute path to the log file 
            string logfile = "Exceptionlog" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";

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

        private static void SaveLog(string message, string filePrefix)
        {
            lock (thisLock)
            {
                message = DateTime.Now + ": " + message + "!\n";
                // string logfile = filePrefix + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                string logfile = filePrefix + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";

                string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string dirPath = rootpath + @"\logs\";
                string path = dirPath + logfile;
                // This text is added only once to the file. 
                if (!File.Exists(path))
                {
                    DirCheck(dirPath);
                    // Create a file to write to. 
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(message);
                    }
                }
                // This text is always added, making the file longer over time 
                // if it is not deleted. 
                else
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(message);
                    }
                }
                //Debug.WriteLine(message);
            }
        }
        private static void SaveLog2(string message, string filePrefix)
        {
            lock (thisLock)
            {
                try
                {
                    message = DateTime.Now + ": " + message + "!\n";
                    // string logfile = filePrefix + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                    //string logfile = filePrefix + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";
                    string logfile = "log.txt";

                    string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                    string dirPath = rootpath + @"\logs\";
                    //string pathroot = @"C:\inetpub\wwwroot\POSSUM\api-v0.possumsystem.com";
                    //string pathroot = @"C:\POSSUM\Newfolder\PossumLogsFoodOrder\";

                    //string dirPath = @"C:\logs\";
                    string path = dirPath + logfile;
                    // This text is added only once to the file. 
                    if (!File.Exists(path))
                    {
                        DirCheck(dirPath);
                        // Create a file to write to. 
                        using (StreamWriter sw = File.CreateText(path))
                        {
                            sw.WriteLine(message);
                        }
                    }
                    // This text is always added, making the file longer over time 
                    // if it is not deleted. 
                    else
                    {
                        using (StreamWriter sw = File.AppendText(path))
                        {
                            sw.WriteLine(message);
                        }
                    }
                    //Debug.WriteLine(message);
                }
                catch (Exception ex)
                {

                }
            }

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


        static string CurrentException = string.Empty;
        private static void Bg_DoWork(object sender, DoWorkEventArgs e)
        {
            SendEmail(CurrentException);
        }
        public static void SendEmail(string message)
        {
            try
            {
                Guid terminalId;
                string connectionString;
                Guid.TryParse(ConfigurationManager.AppSettings["TerminalId"], out terminalId);
                connectionString = ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;

                Terminal terminal = new Terminal();
                Outlet outlet = new Outlet();
                using (var uof = new UnitOfWork(new ApplicationDbContext(connectionString)))
                {
                    var localOutletRepo = uof.OutletRepository;
                    var localTerminalRepo = uof.TerminalRepository;
                    terminal = localTerminalRepo.FirstOrDefault(c => c.Id == terminalId);
                    outlet = localOutletRepo.FirstOrDefault(c => c.Id == terminal.OutletId);
                }

                string receiver = "zahid@deltronsystems.com";// Receiver email
                string subject = outlet.Name + " Exception";


                var client = new DataSMTPClient();

                string html = "";

                StringBuilder str = new StringBuilder();
                str.AppendLine("<div style='padding: 10px 30px 0'>");
                str.AppendLine("<p style ='margin: 0 0 10px; padding: 0; font - size:16px; color:#111111'> Hello, " + "POSSUM" + "</ p >");

                str.AppendLine("<div style = 'width: 100 %; height: 1px; background:#cfcfcf; margin-top:25px; max-height:1px' ></ div >");
                str.AppendLine("<hr/>");
                str.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111' > Exception on " + terminal.UniqueIdentification + "</ p >");

                str.AppendLine("<p style = 'margin: 0 0 10px; padding: 0; font - size:16px; color:#111111;' > " + message + "</ p >");

                str.AppendLine("</div>");

                html = str.ToString();
                client.SendMail(receiver, subject, message, html);
            }
            catch (Exception ex)
            {
                //
            }


        }
        #endregion
    }
}
