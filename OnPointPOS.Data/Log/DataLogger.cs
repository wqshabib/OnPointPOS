using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class DataLogger
    {
        public static void SendBookingEmail(string subject, string text)
        {
            try
            {
                var client = new DataSMTPClient();
                client.SendMail("waqas.habib@ewolution.se", "POSSUM-Booking - " + subject, text, text);
            }
            catch (Exception ex)
            {

            }
        }

        public static void Exception(Exception ex)
        {
            try
            {
                var client = new DataSMTPClient();
                client.SendMail("waqas.habib@ewolution.se", "POSSUM-Exception", ex.ToLog(), ex.ToLog());
            }
            catch (Exception exx)
            {

            }
        }
    }

    public static class ExceptionExtensions
    {
        public static string ToLog(this Exception exception)
        {
            Exception e = exception;
            StringBuilder s = new StringBuilder();
            while (e != null)
            {
                s.AppendLine("Exception type: " + e.GetType().FullName);
                s.AppendLine("Message       : " + e.Message);
                s.AppendLine("Stacktrace:");
                s.AppendLine(e.StackTrace);
                s.AppendLine();
                e = e.InnerException;
            }
            return s.ToString();
        }
    }
}
