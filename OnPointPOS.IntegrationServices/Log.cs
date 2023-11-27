using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Integration.Services
{
    public class Log
    {
        #region Save Log
        private static Object thisLock = new Object();
        public static void WriteLog(string message, string log = "log")
        {
            SaveLog(message, log);
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
            string logfile = "log" + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".txt";

            string rootpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string dirPath = rootpath + @"\logs\";
            string path = dirPath + logfile;
            // This text is added only once to the file. 

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
        #endregion
    }
}

