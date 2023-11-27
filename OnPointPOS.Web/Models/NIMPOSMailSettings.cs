using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace POSSUM.Web
{
    public class NIMPOSMailSettings
	{   
       // private static string host = string.IsNullOrEmpty(ConfigurationManager.AppSettings["SMTP"]) ? "mail.westbahr.com" : ConfigurationManager.AppSettings["SMTP"];
		private static string host = string.IsNullOrEmpty(ConfigurationManager.AppSettings["SMTP"]) ? "smtp.office365.com" : ConfigurationManager.AppSettings["SMTP"];
		private static readonly int port = 587;// 25;
       
        private static void SendMail(MailMessage mail)
        {
            var client = new SmtpClient();
            client.Port = port;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;			
			client.Credentials = new System.Net.NetworkCredential("report@nimpos.com", "NimRPLuq21");//"report@nimpos.com", "Moko0084"
			client.DeliveryMethod = SmtpDeliveryMethod.Network;
			client.EnableSsl = true;
			client.Host = host;
            client.Send(mail);
        }

        public static SendMailResult SendMail(string subject, string body, List<string> emails, List<MailFile> files, bool html = true, string sender= "report@nimpos.com")
        {
            var mail = new MailMessage();
            mail.Subject = subject;
            mail.Body = body;
            mail.BodyEncoding = Encoding.UTF8;
            mail.Sender = new MailAddress(sender);
            mail.From = new MailAddress(sender);
            mail.IsBodyHtml = html;
            files.ForEach(e => mail.Attachments.Add(e.Attach));
            emails.ForEach(e => mail.To.Add(new MailAddress(e)));
            try
            {
                SendMail(mail);
            }
            catch(Exception e)
            {
              
                return new SendMailResult(false, e);
            }
            return new SendMailResult(true, null);
        }

        public class SendMailResult
        {
            public SendMailResult()
            {

            }

            public SendMailResult(bool result, Exception exception)
            {
                this.Result = result;
                this.Exception = exception;
            }

            public bool Result { get; set; }

            public Exception Exception { get; set; }
        }

        public class MailFile
        {
            private Stream file;
            private String name;
            private String type;

            public MailFile(Stream file, String name, String type)
            {
                this.file = file;
                this.name = name;
                this.type = type;
            }

            public Attachment Attach
            {
                get
                {
                    this.file.Position = 0;
                    return new Attachment(file, name, type);
                }
            }

            public Stream File { get { return file; } }
            public String Name { get { return name; } }
            public String Type { get { return type; } }
        }
    }
}
