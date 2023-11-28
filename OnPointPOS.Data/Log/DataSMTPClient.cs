using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Net.Mime;
using System.Configuration;

namespace POSSUM.Data
{

    public class DataSMTPClient
    {
        #region [Private Members]

        /// <summary>
        /// 
        /// </summary>
        private SmtpClient _client = null;

        /// <summary>
        /// 
        /// </summary>
        const char addressDelimeter = ';';

        #endregion

        #region [Private Methods]

        /// <summary>
        /// Initializes the client.
        /// </summary>
        private void InitializeClient()
        {
            if (_client == null)
            {

                _client = new SmtpClient("smtp.gmail.com");  //smtp.office365.com
                var mailAddressFrom = ConfigurationManager.AppSettings["MailAddressFrom"];
                var emailPassword = ConfigurationManager.AppSettings["EmailPassword"];
                ICredentials credentials = new System.Net.NetworkCredential(mailAddressFrom, emailPassword);
                // ICredentials credentials = new System.Net.NetworkCredential("irmofr@gmail.com", "Luqon@123");


                _client.Credentials = (System.Net.NetworkCredential)credentials;
                int? port = int.Parse("587");

                if (port.HasValue && port.Value > 0)
                {
                    _client.Port = port.Value;
                }

                bool enableSSL = true;
                string ssl = "true";
                if (ssl != null)
                {
                    if (!bool.TryParse(ssl, out enableSSL))
                    {
                        enableSSL = false;
                    }
                }

                _client.EnableSsl = enableSSL;
            }
        }

        public DataSMTPClient() 
        {
            InitializeClient();
        }

        #endregion //[Private Methods]

        #region [Public Methods]





        /// <summary>
        /// Sends mail to one or more recipients
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="attachments">The attachments.</param>
        public void SendMail(string to, string subject, string text, string html)
        {
            AlternateView plainView = null;
            if (!string.IsNullOrEmpty(text))
            {
                plainView = AlternateView.CreateAlternateViewFromString(text, Encoding.UTF8, "text/plain");
            }

            AlternateView htmlView = null;
            if (!string.IsNullOrEmpty(html))
            {
                htmlView = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, "text/html");
            }




            MailMessage mailMessage = new MailMessage();
            //MailAddress from = new MailAddress(ConfigurationManager.AppSettings["MailAddressFrom"]);
            //mailMessage.From = from;

            if (to != null)
            {
                List<String> toAddresses = to.Replace(", ", ",").Split(',').ToList();

                foreach (String toAddress in toAddresses)
                {
                    mailMessage.To.Add(new MailAddress(toAddress));
                }
            }
            string cc = ConfigurationManager.AppSettings["MailAddressCC"];
            if (!string.IsNullOrEmpty(cc))
            {
                List<String> ccAddresses = cc.Replace(", ", ",").Split(',').Where(y => y != "").ToList();

                foreach (String ccAddress in ccAddresses)
                {
                    mailMessage.CC.Add(new MailAddress(ccAddress));
                }
            }

            mailMessage.Subject = subject;
            mailMessage.SubjectEncoding = Encoding.UTF8;

            mailMessage.Body = text;
            if (!string.IsNullOrEmpty(text))
            {
                mailMessage.AlternateViews.Add(plainView);
            }

            if (html != null)
            {
                mailMessage.AlternateViews.Add(htmlView);
            }

            mailMessage.Sender = new MailAddress("deltron.pakistan@gmail.com");
            mailMessage.From = new MailAddress("deltron.pakistan@gmail.com");
            if (_client == null)
            {
                InitializeClient();
            }

            _client.Send(mailMessage);


        }

        #endregion //[Public Methods]
    }
}
