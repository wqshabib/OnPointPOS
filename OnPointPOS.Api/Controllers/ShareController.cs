using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Web.Http;
using System.Diagnostics;
using POSSUM.Model;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Web;
using POSSUM.Api.Models;

namespace POSSUM.Api.Controllers
{
    [System.Web.Http.RoutePrefix("api/Share")]

    public class ShareController : BaseAPIController
    {
        public ShareController()
        {
            
        }

        [HttpPost]
        [Route("ShareReceipt")]
        public IHttpActionResult ShareReceipt(ShareReceipt shareReceipt)
        {
            try
            {
                if (shareReceipt.ShareMode.Equals("Email"))
                {
                    Order order = shareReceipt.Order;
                    ICollection<OrderLine> orderLines = shareReceipt.Order.OrderLines;
                    ICollection<Payment> payments = shareReceipt.Order.Payments;

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("<html><head><style> * { font-family: \"Courier New\", \"Courier New\", monospace; } </style></head>");
                    stringBuilder.AppendLine("<body style=\"width: 400px; padding: 10px; overflow: auto; border: 1px solid black;\">");
                    stringBuilder.AppendLine("<div>");
                    stringBuilder.AppendLine("<div style=\"width: 40%; float: left;\">Receipt No</div>");
                    stringBuilder.AppendLine("<div style=\"width: 60%; text-align: right; float: right;\">" + order.ReceiptNumber + "</div>");
                    stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                    stringBuilder.AppendLine("</div>");
                    stringBuilder.AppendLine("<div>");
                    stringBuilder.AppendLine("<div style=\"width: 40%; float: left;\">Order No</div>");
                    stringBuilder.AppendLine("<div style=\"width: 60%; text-align: right; float: right;\">" + (!string.IsNullOrEmpty(order.OrderNoOfDay) ? order.OrderNoOfDay : string.Empty) + "</div>");
                    stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                    stringBuilder.AppendLine("</div>");
                    stringBuilder.AppendLine("<div>");
                    stringBuilder.AppendLine("<div style=\"width: 40%; float: left;\">Total</div>");
                    stringBuilder.AppendLine("<div style=\"width: 60%; text-align: right; float: right;\">" + order.OrderTotal + " kr</div>");
                    stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                    stringBuilder.AppendLine("</div>");
                    stringBuilder.AppendLine("<div>");
                    stringBuilder.AppendLine("<div style=\"width: 40%; float: left;\">Status</div>");
                    stringBuilder.AppendLine("<div style=\"width: 60%; text-align: right; float: right;\">" + order.Status + "</div>");
                    stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                    stringBuilder.AppendLine("</div>");
                    stringBuilder.AppendLine("<div>");
                    stringBuilder.AppendLine("<div style=\"width: 40%; float: left;\">Date</div>");
                    stringBuilder.AppendLine("<div style=\"width: 60%; text-align: right; float: right;\">" + (order.CreationDate != null ? order.CreationDate.ToString("dd/MM/yyyy h:mm tt") : string.Empty) + "</div>");
                    stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                    stringBuilder.AppendLine("</div>");
                    if (order.Customer != null && !string.IsNullOrEmpty(order.Customer.Name))
                    {
                        stringBuilder.AppendLine("<div>");
                        stringBuilder.AppendLine("<div style=\"width: 40%; float: left;\">Customer Name</div>");
                        stringBuilder.AppendLine("<div style=\"width: 60%; text-align: right; float: right;\">" + order.Customer.Name + "</div>");
                        stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                        stringBuilder.AppendLine("</div>");
                    }
                    if (!string.IsNullOrEmpty(order.Comments))
                    {
                        stringBuilder.AppendLine("<div>");
                        stringBuilder.AppendLine("<div style=\"width: 40%; float: left;\">Comments</div>");
                        stringBuilder.AppendLine("<div style=\"width: 60%; text-align: right; float: right;\">" + order.Comments + "</div>");
                        stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                        stringBuilder.AppendLine("</div>");
                    }
                    stringBuilder.AppendLine("<p>Items</p>");
                    stringBuilder.AppendLine("<div>");
                    stringBuilder.AppendLine("<div style=\"width: 46.8%; float: left; border: 1px solid black;\">Items Name</div>");
                    stringBuilder.AppendLine("<div style=\"width: 26%; float: left; text-align: center; border-width: 1px 0px 1px 0px; border-color: black; border-style: solid;\">Quantity</div>");
                    stringBuilder.AppendLine("<div style=\"width: 26%; float: left; text-align: center; border: 1px solid black;\">Price</div>");
                    stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                    stringBuilder.AppendLine("</div>");
                    if (orderLines != null && orderLines.Count > 0)
                    {
                        //Adding Items
                        stringBuilder.AppendLine("<div style=\"margin-top: 10px;\">");
                        foreach (var orderLine in orderLines)
                        {
                            stringBuilder.AppendLine("<div style=\"margin-bottom: 5px;\">");
                            stringBuilder.AppendLine("<div style=\"width: 100%;\">");
                            stringBuilder.AppendLine("<div style=\"width: 46.8%; float: left;\">" + orderLine.ItemName + "</div>");
                            stringBuilder.AppendLine("<div style=\"width: 26%; float: left; text-align: center;\">" + orderLine.Quantity + "</div>");
                            stringBuilder.AppendLine("<div style=\"width: 26%; float: left; text-align: center;\">" + orderLine.GrossAmountDiscounted() + " kr</div>");
                            stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                            stringBuilder.AppendLine("</div>");
                            if (orderLine.ItemDiscount != 0)
                            {
                                stringBuilder.AppendLine("<div style=\"width: 100%;\">");
                                stringBuilder.AppendLine("<div style=\"width: 72.8%; float: left;\">Discount</div>");
                                stringBuilder.AppendLine("<div style=\"width: 26%; float: left; text-align: center;\">" + orderLine.ItemDiscount + " kr</div>");
                                stringBuilder.AppendLine("<div style=\"clear: both;\"></div>");
                                stringBuilder.AppendLine("</div>");
                            }
                            if (!string.IsNullOrEmpty(orderLine.ItemComments))
                            {
                                stringBuilder.AppendLine("<div style=\"width: 100%; color: #808080\">Comments: " + orderLine.ItemComments + "</div>");
                            }
                            stringBuilder.AppendLine("</div>");
                        }
                        stringBuilder.AppendLine("</div>");
                        //End Adding Items
                    }

                    var isMailSent = new SMTPClient().SendMail(shareReceipt.ShareWith, "", "Order Receipt", "", stringBuilder.ToString());

                    if (isMailSent)
                        return StatusCode(HttpStatusCode.OK);
                    else
                        return StatusCode(HttpStatusCode.ExpectationFailed);
                }
                else
                {
                    return StatusCode(HttpStatusCode.NotImplemented);
                }
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }


        [HttpPost]
        [Route("ShareFile")]
        public IHttpActionResult ShareFile()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;

                var email = httpRequest.Form.Get("Email");
                var subject = httpRequest.Form.Get("Subject");
                var fileName = httpRequest.Form.Get("FileName");

                List<MailFile> mailFiles = new List<MailFile>();

                if (httpRequest.Files.Count > 0)
                {
                    var attachedFile = httpRequest.Files[0].InputStream;
                    mailFiles.Add(new MailFile(attachedFile, fileName, "*/*"));
                }

                var isMailSent = new SMTPClient().SendMailWithFiles(email, "", subject, "", "", mailFiles);

                if (isMailSent)
                    return StatusCode(HttpStatusCode.OK);

                return StatusCode(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                LogWriter.LogWrite(ex);
                Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                return StatusCode(HttpStatusCode.ExpectationFailed);
            }
        }
    }
}
