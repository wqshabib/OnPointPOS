using Newtonsoft.Json;
using POSSUM.ApiModel;
using POSSUM.Data;
using POSSUM.Model;
using POSSUM.StandAloneApi.Handlers;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace POSSUM.StandAloneApi.Controllers
{
    [Authorize]
    [System.Web.Http.RoutePrefix("mapi/SwishOrder")]
    public class SwishOrderController : BaseAPIController
    {
        private string connectionString = "";
        bool isAuthenticated = false;

        public SwishOrderController()
        {
            connectionString = GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
                isAuthenticated = true;
        }

        [System.Web.Http.HttpPost]
        [Route("PostSwishOrder")]
        public HttpResponseMessage PostSwishOrder(SwishOrderApi swishOrder)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        var dbOrder = db.OrderMaster.FirstOrDefault(obj => obj.Id == swishOrder.Order.Id);
                        if (dbOrder == null)
                        {
                            dbOrder = OrderApi.ConvertApiModelToModel(swishOrder.Order);
                            db.OrderMaster.Add(dbOrder);
                        }
                        else
                        {
                            dbOrder = OrderApi.UpdateModel(dbOrder, swishOrder.Order);
                            db.Entry(dbOrder).State = EntityState.Modified;
                        }

                        foreach (var orderLine in swishOrder.Order.OrderLines)
                        {
                            var dbOrderLine = db.OrderDetail.FirstOrDefault(obj => obj.Id == orderLine.Id);
                            if (dbOrderLine == null)
                            {
                                dbOrderLine = OrderLineApi.ConvertApiModelToModel(orderLine);
                                db.OrderDetail.Add(dbOrderLine);
                            }
                            else
                            {
                                dbOrderLine = OrderLineApi.UpdateModel(dbOrderLine, orderLine);
                                db.Entry(dbOrderLine).State = EntityState.Modified;
                            }
                        }

                        var dbSwishPayment = db.SwishPayment.FirstOrDefault(obj => obj.OrderId == swishOrder.Order.Id);
                        if (dbSwishPayment == null)
                        {
                            dbSwishPayment = new SwishPayment();
                            dbSwishPayment.Id = Guid.NewGuid();
                            dbSwishPayment.OrderId = swishOrder.Order.Id;

                            db.SwishPayment.Add(dbSwishPayment);
                        }
                        else
                        {
                            db.Entry(dbSwishPayment).State = EntityState.Modified;
                        }

                        var swishResponse = CreatePaymentRequest(swishOrder);

                        string swishResponseJson = JsonConvert.SerializeObject(swishResponse);
                        LogWriter.LogWrite("SwishOrderController: PostSwishOrder: Json: " + swishResponseJson);

                        swishOrder.Status = swishResponse.Status;
                        swishOrder.Message = swishResponse.Message;

                        if (swishResponse.Status == 0)
                        {
                            dbSwishPayment.SwishId = swishResponse.Id;
                            dbSwishPayment.SwishPaymentStatus = 1;
                            dbSwishPayment.SwishLocation = swishResponse.Location;
                        }

                        db.SaveChanges();

                        swishOrder.Order.SwishPayment = SwishPaymentApi.ConvertModelToApiModel(dbSwishPayment);

                        var jsonObject = JsonConvert.SerializeObject(swishOrder);
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        httpResponseMessage.Content = new StringContent(jsonObject, Encoding.UTF8, "application/json");

                        return httpResponseMessage;
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }

        [System.Web.Http.HttpGet]
        [Route("GetSwishOrderStatus")]
        public HttpResponseMessage GetSwishOrderStatus(string orderId)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        SwishOrderApi swishOrder = new SwishOrderApi();
                        Guid orderGuid = Guid.Parse(orderId);

                        var dbOrder = db.OrderMaster.FirstOrDefault(obj => obj.Id == orderGuid);
                        var dbSwishPayment = db.SwishPayment.FirstOrDefault(obj => obj.OrderId == orderGuid);

                        swishOrder.Order = OrderApi.ConvertModelToApiModel(dbOrder);
                        if (dbSwishPayment != null)
                            swishOrder.Order.SwishPayment = SwishPaymentApi.ConvertModelToApiModel(dbSwishPayment);

                        var jsonObject = JsonConvert.SerializeObject(swishOrder);
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        httpResponseMessage.Content = new StringContent(jsonObject, Encoding.UTF8, "application/json");

                        return httpResponseMessage;
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }

        [System.Web.Http.HttpGet]
        [Route("CancelSwishOrder")]
        public HttpResponseMessage CancelSwishOrder(string orderId)
        {
            if (isAuthenticated)
            {
                #if DEBUG
                connectionString = connectionString + " Integrated Security=SSPI; persist security info=True;";
                #endif

                try
                {
                    using (ApplicationDbContext db = new ApplicationDbContext(connectionString))
                    {
                        SwishOrderApi swishOrder = new SwishOrderApi();
                        Guid orderGuid = Guid.Parse(orderId);

                        var dbOrder = db.OrderMaster.FirstOrDefault(obj => obj.Id == orderGuid);
                        var dbSwishPayment = db.SwishPayment.FirstOrDefault(obj => obj.OrderId == dbOrder.Id);

                        var swishResponse = CancelPaymentRequest(dbSwishPayment.SwishLocation);

                        string swishResponseJson = JsonConvert.SerializeObject(swishResponse);
                        LogWriter.LogWrite("SwishOrderController: CancelSwishOrder: Json: " + swishResponseJson);

                        if (!string.IsNullOrEmpty(swishResponse.id))
                        {
                            dbSwishPayment.SwishId = swishResponse.id;
                            dbSwishPayment.SwishPaymentId = swishResponse.paymentReference;
                            dbSwishPayment.SwishPaymentStatus = 3;
                            dbSwishPayment.SwishResponse = swishResponseJson;

                            swishOrder.Status = 0;
                            swishOrder.Message = swishResponse.status;

                            db.Entry(dbSwishPayment).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            dbSwishPayment.SwishPaymentStatus = 4;

                            swishOrder.Status = -1;
                            swishOrder.Message = swishResponse.ErrorMessage;
                        }

                        swishOrder.Order = OrderApi.ConvertModelToApiModel(dbOrder);
                        swishOrder.Order.SwishPayment = SwishPaymentApi.ConvertModelToApiModel(dbSwishPayment);

                        var jsonObject = JsonConvert.SerializeObject(swishOrder);
                        var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                        httpResponseMessage.Content = new StringContent(jsonObject, Encoding.UTF8, "application/json");

                        return httpResponseMessage;
                    }
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite(ex);
                    Debug.Listeners.Add(new TextWriterTraceListener(@"C:\temp\error2.txt"));
                    return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
                }
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }

        private PaymentRequestECommerceResponse CreatePaymentRequest(SwishOrderApi swishOrder)
        {
            try
            {
                string callBackUrl = ConfigurationManager.AppSettings["PayBaseUrl"] + "SwishCallback/CallBack";
                var swishMerchantID = ConfigurationManager.AppSettings["SwishMerchantID"];
                var swishEnvironment = ConfigurationManager.AppSettings["SwishEnvironment"];

                ClientCertificate clientCertificate = GetSwishCertificates();

                SwishPaymentHandler swishPayment = new SwishPaymentHandler(clientCertificate, callBackUrl, swishMerchantID, swishEnvironment);

                OrderApi order = swishOrder.Order;

                string orderId = order.Id.ToString("N");
                string companyId = this.Company.Id.ToString();
                string instructionUUID = Guid.NewGuid().ToString("N").ToUpper();

                //return swishPayment.MakePaymentRequest(orderId, order.OrderTotal, swishOrder.PhoneNo, companyId, instructionUUID);
                return swishPayment.MakePaymentRequest(orderId, swishOrder.SwishAmount, swishOrder.PhoneNo, companyId, instructionUUID);
            }
            catch (Exception e)
            {
                LogWriter.LogWrite(e);
                return null;
            }
        }

        private CancelPaymentResponse CancelPaymentRequest(string paymentLocationURL)
        {
            try
            {
                var swishEnvironment = ConfigurationManager.AppSettings["SwishEnvironment"];

                ClientCertificate clientCertificate = GetSwishCertificates();

                SwishPaymentHandler swishPayment = new SwishPaymentHandler(clientCertificate, swishEnvironment);
                return swishPayment.CancelPaymentRequest(paymentLocationURL);
            }
            catch (Exception e)
            {
                LogWriter.LogWrite(e);
                return null;
            }
        }

        private ClientCertificate GetSwishCertificates()
        {
            try
            {
                var baseCertificatesPath = ConfigurationManager.AppSettings["SwishCertificatePath"];
                var swishCertificate = ConfigurationManager.AppSettings["SwishCertificateName"];
                var certificateAsStream = ConfigurationManager.AppSettings["CertificateAsStream"];
                var useMachineKeySet = ConfigurationManager.AppSettings["UseMachineKeySet"];

                ClientCertificate clientCertificate = new ClientCertificate();
                //clientCertificate.CertificateFilePath = baseCertificatesPath + swishCertificate;
                clientCertificate.CertificateAsStream = File.OpenRead(baseCertificatesPath + swishCertificate);
                clientCertificate.Password = "swish";
                if (useMachineKeySet.Equals("1"))
                    clientCertificate.UseMachineKeySet = true;
                else
                    clientCertificate.UseMachineKeySet = false;

                return clientCertificate;
            }
            catch (Exception e)
            {
                LogWriter.LogWrite(e);
                return null;
            }
        }
    }
}
