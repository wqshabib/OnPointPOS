using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using POSSUM.Api.Models;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.UI;

namespace POSSUM.Api.Controllers
{
    [RoutePrefix("api/Notification")]
    public class NotificationController : ApiController
    {

        [HttpPost]
        public HttpResponseMessage Post(NotificationModel model)
        {
            try
            {
                if (model.NotificationType == NotificationType.Email)
                {
                    var cc = ConfigurationManager.AppSettings["MailAddressCC"];
                    new SMTPClient().SendMail(model.Email, cc, "Subject", "Text", "html");
                    //manage email code here
                }
                if (model.NotificationType == NotificationType.SMS)
                {
                    //manage sms code here
                }
                return Request.CreateResponse(HttpStatusCode.OK, "Success");

            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Exception ! " + e.ToString());
            }

        }

        [Obsolete]
        public static void GenerateInvoicePDF()
        {
            //Dummy data for Invoice (Bill).
            string companyName = "ASPSnippets";
            int orderNo = 2303;
            DataTable dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[4] {
                        new DataColumn("Name", typeof(string)),
                        new DataColumn("Quantity", typeof(string)),
                        new DataColumn("Price", typeof(int)),
                        new DataColumn("Total",  typeof(int))});


            dt.Rows.Add(101, "Sun Glasses", 200, 5, 1000);
            dt.Rows.Add(102, "Jeans", 400, 2, 800);
            dt.Rows.Add(103, "Trousers", 300, 3, 900);
            dt.Rows.Add(104, "Shirts", 550, 2, 1100);

            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {
                    StringBuilder sb = new StringBuilder();

                    //Generate Invoice (Bill) Header.
                    sb.Append("<table width='100%' cellspacing='0' cellpadding='2'>");
                    sb.Append("<tr><td align='center' style='background-color: #18B5F0' colspan = '2'><b>Order Sheet</b></td></tr>");
                    sb.Append("<tr><td colspan = '2'></td></tr>");
                    sb.Append("<tr><td><b>Order No: </b>");
                    sb.Append(orderNo);
                    sb.Append("</td><td align = 'right'><b>Date: </b>");
                    sb.Append(DateTime.Now);
                    sb.Append(" </td></tr>");
                    sb.Append("<tr><td colspan = '2'><b>Company Name: </b>");
                    sb.Append(companyName);
                    sb.Append("</td></tr>");
                    sb.Append("</table>");
                    sb.Append("<br />");

                    //Generate Invoice (Bill) Items Grid.
                    sb.Append("<table border = '1'>");
                    sb.Append("<tr>");
                    foreach (DataColumn column in dt.Columns)
                    {
                        sb.Append("<th style = 'background-color: #D20B0C;color:#ffffff'>");
                        sb.Append(column.ColumnName);
                        sb.Append("</th>");
                    }
                    sb.Append("</tr>");

                    foreach (DataRow row in dt.Rows)
                    {
                        sb.Append("<tr>");
                        foreach (DataColumn column in dt.Columns)
                        {
                            sb.Append("<td>");
                            sb.Append(row[column]);
                            sb.Append("</td>");
                        }
                        sb.Append("</tr>");
                    }
                    sb.Append("<tr><td align = 'right' colspan = '");
                    sb.Append(dt.Columns.Count - 1);
                    sb.Append("'>Total</td>");
                    sb.Append("<td>");
                    sb.Append(dt.Compute("sum(Total)", ""));
                    sb.Append("</td>");
                    sb.Append("</tr></table>");

                    //Export HTML String as PDF.
                    StringReader sr = new StringReader(sb.ToString());
                    
                    var folderPath = ConfigurationManager.AppSettings["PDFInvoiceToSavePath"];
                    using (FileStream stream = new FileStream(folderPath + "Filename", FileMode.Create))
                    {
                        Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                        HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                        PdfWriter.GetInstance(pdfDoc, stream);
                        pdfDoc.Open();
                        htmlparser.Parse(sr);                        
                        pdfDoc.Close();
                        stream.Close();
                    }
                   
                    var mailfiles = new List<MailFile>();
                    var memstream = new MemoryStream();
                    using (FileStream fs = System.IO.File.OpenRead(folderPath + "Filename"))
                    {
                        fs.CopyTo(memstream);
                    }
                    memstream.Position = 0;
                    mailfiles.Add(new MailFile(memstream, "Invoice.pdf", "application/pdf"));
                   
                    string subject = "Invoice: ";
                    string comment = "Test comments";
                    string body = "<span>" + comment + "</span><br />";
                    body += "<span>från: " + "dtFrom" + "</span><br />";
                    body += "<span>till: " + "dtTo" + "</span><br />";

                    List<string> emails = new List<string>();
                    emails.Add("khaliljandran@gmail.com");

                    var cc = ConfigurationManager.AppSettings["MailAddressCC"];

                    new SMTPClient().SendMail(emails, cc, subject, body, mailfiles, true);
                }
            }
        }
    }

}
