using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace POSSUM.Api.Models
{
    
    public class NotificationModel
    {

        public Order Receipt { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public NotificationType NotificationType { get; set; }
    }
    public enum NotificationType
    {
        Email,
        SMS
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