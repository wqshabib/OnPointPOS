using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using POSSUM.Api.Models;

namespace POSSUM.Api
{

    public class SMTPClient
    {
        private SmtpClient smtpClient = null;
        private int? port;
        private readonly string smtp;
        private readonly string mailAddressFrom;
        private readonly string mailAddressPassword;
        private bool enableSSL = true;
        private string ssl = "true";

        public SMTPClient()
        {
            smtp = string.IsNullOrEmpty(ConfigurationManager.AppSettings["SMTP"]) ? "smtp.office365.com" : ConfigurationManager.AppSettings["SMTP"];
            port = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Port"]) ? 587 : Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            mailAddressFrom = ConfigurationManager.AppSettings["MailAddressFrom"];
            mailAddressPassword = ConfigurationManager.AppSettings["EmailPassword"];
        }

        public SMTPClient(string smtp, int? port, string mailAddressFrom, string mailAddressPassword)
        {
            this.smtp = smtp;
            this.port = port;
            this.mailAddressFrom = mailAddressFrom;
            this.mailAddressPassword = mailAddressPassword;
        }

        private void initSMTPClient()
        {
            if (smtpClient == null)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                                      | SecurityProtocolType.Tls11
                                      | SecurityProtocolType.Tls12;

                smtpClient = new SmtpClient(smtp);

                ICredentials credentials = new NetworkCredential(mailAddressFrom, mailAddressPassword);

                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = (NetworkCredential)credentials;

                if (port.HasValue && port.Value > 0)
                    smtpClient.Port = port.Value;

                if (!bool.TryParse(ssl, out enableSSL))
                    enableSSL = false;

                smtpClient.EnableSsl = enableSSL;
            }
        }

        public bool SendMail(string to, string cc, string subject, string text, string html)
        {
            try
            {
                AlternateView plainView = null;
                if (!string.IsNullOrEmpty(text))
                    plainView = AlternateView.CreateAlternateViewFromString(text, Encoding.UTF8, "text/plain");

                AlternateView htmlView = null;
                if (!string.IsNullOrEmpty(html))
                    htmlView = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, "text/html");


                MailMessage mailMessage = new MailMessage();
                MailAddress from = new MailAddress(mailAddressFrom);
                mailMessage.From = from;

                if (to != null)
                {
                    List<String> toAddresses = to.Replace(", ", ",").Split(',').ToList();

                    foreach (String toAddress in toAddresses)
                    {
                        mailMessage.To.Add(new MailAddress(toAddress));
                    }
                }

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
                    mailMessage.AlternateViews.Add(plainView);

                if (html != null)
                    mailMessage.AlternateViews.Add(htmlView);

                if (smtpClient == null)
                    initSMTPClient();

                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SendMailWithFiles(string to, string cc, string subject, string text, string html, List<MailFile> mailFiles)
        {
            try
            {
                AlternateView plainView = null;
                if (!string.IsNullOrEmpty(text))
                    plainView = AlternateView.CreateAlternateViewFromString(text, Encoding.UTF8, "text/plain");

                AlternateView htmlView = null;
                if (!string.IsNullOrEmpty(html))
                    htmlView = AlternateView.CreateAlternateViewFromString(html, Encoding.UTF8, "text/html");


                MailMessage mailMessage = new MailMessage();
                MailAddress from = new MailAddress(mailAddressFrom);
                mailMessage.From = from;

                if (to != null)
                {
                    List<String> toAddresses = to.Replace(", ", ",").Split(',').ToList();

                    foreach (String toAddress in toAddresses)
                    {
                        mailMessage.To.Add(new MailAddress(toAddress));
                    }
                }

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
                    mailMessage.AlternateViews.Add(plainView);

                if (!string.IsNullOrEmpty(html))
                    mailMessage.AlternateViews.Add(htmlView);

                if (mailFiles.Count > 0)
                    mailFiles.ForEach(e => mailMessage.Attachments.Add(e.Attach));

                if (smtpClient == null)
                    initSMTPClient();

                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool SendMail(List<string> to, string cc, string subject, string body, List<MailFile> files, bool html = true)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(mailAddressFrom);
                mailMessage.Sender = new MailAddress(mailAddressFrom);
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.IsBodyHtml = html;

                if (files.Count > 0)
                    files.ForEach(e => mailMessage.Attachments.Add(e.Attach));

                if (to.Count > 0)
                    to.ForEach(e => mailMessage.To.Add(new MailAddress(e)));

                if (!string.IsNullOrEmpty(cc))
                {
                    List<String> ccAddresses = cc.Replace(", ", ",").Split(',').Where(y => y != "").ToList();
                    
                    if (ccAddresses.Count > 0)
                        ccAddresses.ForEach(cca => mailMessage.CC.Add(new MailAddress(cca)));
                }

                if (smtpClient == null)
                    initSMTPClient();

                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
