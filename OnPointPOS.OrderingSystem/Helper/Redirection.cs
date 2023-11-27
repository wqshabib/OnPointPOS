using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace ML.Rest2.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public static class Redirection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="currentURL"></param>
        /// <param name="rest2Method"></param>
        /// <returns></returns>
        public static bool IsValid(string companyId, string currentURL, string rest2Method, string mofrURL)
        {
            string message = "";
            try
            {
                message = "API Redirection is called for following company - " + 
                    companyId + " with following Rest2 URL: " + 
                    currentURL + " and with following rest2 method " + rest2Method +
                    " and mofr URL generated is : http://api.mofr.se/" + mofrURL;
                var redirectCompanies = ConfigurationManager.AppSettings["RedirectCompanies"];
                message = message + " - Companies List: " + redirectCompanies;

                if (!string.IsNullOrEmpty(redirectCompanies) && !string.IsNullOrEmpty(companyId))
                {
                    redirectCompanies = redirectCompanies.ToLower();
                    companyId = companyId.ToLower();
                    var lst = redirectCompanies.Split(',').ToList();
                    var result = lst.Contains(companyId);
                    message = message + " - Result = True";
                    SendEmail("Redirection Notice", message);
                    return true;
                }

                message = message + " - Result = False";
                SendEmail("Redirection Notice", message);
                return false;
            }
            catch (Exception ex)
            {
                message = message + " - Result = Exception - " + ex.Message;
                SendEmail("Redirection Notice", message);
            }

            return false;
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="subject"></param>
       /// <param name="message"></param>
        public static void SendEmail(string subject, string message)
        {
            try
            {
                //var client = new SMTPClient();
                //var mailAddressFrom = new MailAddress("wqshabib@gmail.com");
                //IList<EmailAttachment> attachments = new List<EmailAttachment>();

                //string receiver = "";
                //receiver = "wqshabib@gmail.com,muhammad.khalil@luqon.com";
                //var redirectDebugEmail = Convert.ToBoolean(ConfigurationManager.AppSettings["RedirectDebugEmail"]);
                //if(redirectDebugEmail)
                //    client.SendMail(mailAddressFrom, receiver, subject, "", message, "", "", attachments);
            }
            catch (Exception ex)
            {
                //
            }
        }
    }
}