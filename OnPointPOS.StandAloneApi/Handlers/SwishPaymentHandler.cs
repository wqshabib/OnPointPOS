using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace POSSUM.StandAloneApi.Handlers
{
    public class SwishPaymentHandler
    {
        readonly string _environment;
        readonly string _merchantAlias;
        readonly string _callbackUrl;
        readonly ClientCertificate _certificate;

        /// <summary>
        /// Construct a E-Commerce client for Swish Payment, with certificate file
        /// </summary>
        /// <param name="clientCertificate">Client Certificate object</param>
        public SwishPaymentHandler(ClientCertificate clientCertificate, string environment = "PROD")
        {
            _certificate = clientCertificate;
            _environment = environment;
        }

        /// <summary>
        /// Construct a E-Commerce client for Swish Payment, with certificate file
        /// </summary>
        /// <param name="clientCertificate">Client Certificate object</param>
        /// <param name="callbackUrl">URL where you like to get the Swish Payment Callback</param>
        /// <param name="merchantAlias">The Swish number of the payee. It needs to match with Merchant Swish number.</param>
        /// <param name="environment">Set what environment of Swish Payment API should be used, PROD, SANDBOX or EMULATOR</param>
        public SwishPaymentHandler(ClientCertificate clientCertificate, string callbackUrl, string merchantAlias, string environment = "PROD")
        {
            _certificate = clientCertificate;
            _environment = environment;
            _callbackUrl = callbackUrl;
            _merchantAlias = merchantAlias;
        }

        /// <summary>
        /// Initiate a Swish Payment Request
        /// </summary>
        /// <param name="payeePaymentReference">Payment reference supplied by theMerchant. This is not used by Swish but is included in responses back to the client. This reference could for example be an order id or similar. If set the value must not exceed 35 characters and only the following characters are allowed: [a-ö, A-Ö, 0-9, -]</param>
        /// <param name="amount">The amount of money to pay. The amount cannot be less than 0.01 SEK and not more than 999999999999.99 SEK. Valid value has to be all digits or with 2 digit decimal separated with a period.</param>
        /// <param name="phoneNo">Phone number of the person paying.</param>
        /// <param name="message">Merchant supplied message about the payment/order. Max 50 characters. Common allowed characters are the letters a-ö, A-Ö, the numbers 0-9, and special characters !?=#$%&()*+,-./:;<'"@. In addition, the following special characters are also allowed: ^¡¢£€¥¿Š§šŽžŒœŸÀÁÂÃÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕØØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõöøùúûüýþÿ.</param>
        /// <param name="instructionUUID">An identifier created by the merchant to uniquely identify a payout instruction sent to the Swish system. Swish uses this identifier to guarantee the uniqueness of a payout instruction and prevent occurrence of unintended double payments. 32 hexadecimal (16- based) digits. Use Guid.NewGuid().ToString("N").ToUpper()</param>
        /// <returns></returns>
        public PaymentRequestECommerceResponse MakePaymentRequest(string payeePaymentReference, decimal amount, string phoneNo, string message, string instructionUUID)
        {
            try
            {
                var requestData = new PaymentRequestECommerceData()
                {
                    payeeAlias = _merchantAlias,
                    currency = "SEK",
                    callbackUrl = _callbackUrl,
                    amount = Math.Round(amount, 2).ToString().Replace(",", "."), // Amount to be paid. Only period/dot (”.”) are accepted as decimal character with maximum 2 digits after. Digits after separator are optional.
                    message = message,
                    payerAlias = phoneNo,
                    payeePaymentReference = payeePaymentReference
                };


                HttpClientHandler handler;
                HttpClient client;

                PrepareHttpClientAndHandler(out handler, out client);

                string requestURL = URL.ProductionPaymentRequest + instructionUUID;

                var httpMethod = HttpMethod.Put;

                switch (_environment)
                {
                    case "EMULATOR":
                        requestURL = URL.EmulatorPaymentRequest + instructionUUID;
                        break;
                    case "SANDBOX":
                        requestURL = URL.SandboxPaymentRequest;
                        httpMethod = HttpMethod.Post;
                        break;
                }

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = httpMethod,
                    RequestUri = new Uri(requestURL),
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json")
                };

                httpRequestMessage.Headers.Add("host", httpRequestMessage.RequestUri.Host);

                var response = client.SendAsync(httpRequestMessage).Result;

                string responseId = string.Empty;
                string responseLocation = string.Empty;
                var responseStatus = 0;
                string responseMessage = string.Empty;

                if (response.IsSuccessStatusCode)
                {
                    var headers = response.Headers.ToList();

                    if (headers.Any(x => x.Key == "Location"))
                    {
                        responseLocation = response.Headers.GetValues("Location").FirstOrDefault();
                    }

                    responseId = instructionUUID;
                    responseStatus = 0;
                    responseMessage = "Payment Request Created";
                }
                else
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    var errorObject = JsonConvert.DeserializeObject<PaymentRequestECommerceErrorResponse[]>(readAsStringAsync.Result);
                    responseStatus = -1;
                    responseMessage = errorObject[0].ErrorMessage;
                }

                client.Dispose();
                handler.Dispose();

                return new PaymentRequestECommerceResponse()
                {
                    Id = responseId,
                    Location = responseLocation,
                    Status = responseStatus,
                    Message = responseMessage
                };
            }
            catch (Exception ex)
            {
                return new PaymentRequestECommerceResponse()
                {
                    Status = -2,
                    Message = ex.ToString()
                };
            }
        }

        public CancelPaymentResponse CancelPaymentRequest(string paymentLocationURL)
        {
            try
            {
                var o = new CancelPaymentRequest()
                {
                    op = "replace",
                    path = "/status",
                    value = "cancelled"
                };

                var requestData = new List<CancelPaymentRequest>() { o };

                HttpClientHandler handler;
                HttpClient client;

                PrepareHttpClientAndHandler(out handler, out client);

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = new HttpMethod("PATCH"), //HttpMethod.Patch
                    RequestUri = new Uri(paymentLocationURL),
                    Content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json-patch+json")
                };

                httpRequestMessage.Headers.Add("host", httpRequestMessage.RequestUri.Host);

                var response = client.SendAsync(httpRequestMessage).Result;

                string errorMessage = string.Empty;
                string location = string.Empty;

                CancelPaymentResponse cancelPaymentResponse;

                if (response.IsSuccessStatusCode)
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    string jsonResponse = readAsStringAsync.Result;

                    cancelPaymentResponse = JsonConvert.DeserializeObject<CancelPaymentResponse>(jsonResponse);
                }
                else
                {
                    var readAsStringAsync = response.Content.ReadAsStringAsync();
                    errorMessage = readAsStringAsync.Result;

                    var errorObject = JsonConvert.DeserializeObject<PaymentRequestECommerceErrorResponse[]>(readAsStringAsync.Result);

                    cancelPaymentResponse = new CancelPaymentResponse()
                    {
                        ErrorMessage = errorObject[0].ErrorMessage
                    };
                }

                client.Dispose();
                handler.Dispose();

                return cancelPaymentResponse;
            }
            catch (Exception ex)
            {
                return new CancelPaymentResponse()
                {
                    ErrorMessage = ex.ToString()
                };
            }
        }

        private void PrepareHttpClientAndHandler(out HttpClientHandler handler, out HttpClient client)
        {
            handler = new HttpClientHandler();

            if (_certificate != null)
            {
                if (_certificate.UseMachineKeySet)
                {
                    if (string.IsNullOrEmpty(_certificate.Password))
                    {
                        if (_certificate.SecureStringPassword != null)
                        {
                            var cert = new X509Certificate2(Misc.ReadFully(_certificate.CertificateAsStream), _certificate.SecureStringPassword, X509KeyStorageFlags.MachineKeySet);
                            handler.ClientCertificates.Add(cert);
                        }
                        else
                        {
                            throw new Exception("Certificate password missing set wish needed to use with MachineKeySet");
                        }
                    }
                    else
                    {
                        var cert = new X509Certificate2(Misc.ReadFully(_certificate.CertificateAsStream), _certificate.Password, X509KeyStorageFlags.MachineKeySet);
                        handler.ClientCertificates.Add(cert);
                    }
                }
                else
                {
                    // Got help for this code on https://stackoverflow.com/questions/61677247/can-a-p12-file-with-ca-certificates-be-used-in-c-sharp-without-importing-them-t
                    using (X509Store store = new X509Store(StoreName.CertificateAuthority, StoreLocation.CurrentUser))
                    {
                        store.Open(OpenFlags.ReadWrite);

                        if (string.IsNullOrEmpty(_certificate.CertificateFilePath))
                        {
                            if (string.IsNullOrEmpty(_certificate.Password))
                            {
                                var cert = new X509Certificate2(Misc.ReadFully(_certificate.CertificateAsStream));

                                if (cert.HasPrivateKey)
                                {
                                    handler.ClientCertificates.Add(cert);
                                }
                                else
                                {
                                    store.Add(cert);
                                }
                            }
                            else
                            {
                                var cert = new X509Certificate2(Misc.ReadFully(_certificate.CertificateAsStream), _certificate.Password);

                                if (cert.HasPrivateKey)
                                {
                                    handler.ClientCertificates.Add(cert);
                                }
                                else
                                {
                                    store.Add(cert);
                                }
                            }
                        }
                        else
                        {
                            var certs = new X509Certificate2Collection();

                            certs.Import(_certificate.CertificateFilePath, _certificate.Password, X509KeyStorageFlags.DefaultKeySet);

                            foreach (X509Certificate2 cert in certs)
                            {
                                if (cert.HasPrivateKey)
                                {
                                    handler.ClientCertificates.Add(cert);
                                }
                                else
                                {
                                    store.Add(cert);
                                }
                            }
                        }
                    }
                }
            }

            client = new HttpClient(handler);
        }
    }

    public class ClientCertificate
    {
        public string CertificateFilePath { get; set; }
        public string Password { get; set; }
        public Stream CertificateAsStream { get; set; }
        public bool UseMachineKeySet { get; set; }
        public SecureString SecureStringPassword { get; set; }
    }

    public class PaymentRequestECommerceResponse
    {
        public string Id { get; set; }
        public string Location { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
    }

    public class PaymentRequestECommerceErrorResponse
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string AdditionalInformation { get; set; }
    }

    public class PaymentRequestECommerceData
    {
        public string payeeAlias { get; set; }
        public string currency { get; set; }
        public string callbackUrl { get; set; }
        public string amount { get; set; }
        public string message { get; set; }
        public string payerAlias { get; set; }
        public string payeePaymentReference { get; set; }
    }

    public class CheckPaymentRequestStatusResponse
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string id { get; set; }
        public string payeePaymentReference { get; set; }
        public string paymentReference { get; set; }
        public string callbackUrl { get; set; }
        public string payerAlias { get; set; }
        public string payeeAlias { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime? datePaid { get; set; }
    }

    public class PaymentCallback
    {
        public string id { get; set; }
        public string payeePaymentReference { get; set; }
        public string paymentReference { get; set; }
        public string callbackUrl { get; set; }
        public string payerAlias { get; set; }
        public string payeeAlias { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime? datePaid { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
    }

    public class CancelPaymentRequest
    {
        public string op { get; set; }
        public string path { get; set; }
        public string value { get; set; }
    }

    public class CancelPaymentResponse
    {
        public string id { get; set; }
        public string payeePaymentReference { get; set; }
        public string paymentReference { get; set; }
        public string callbackUrl { get; set; }
        public string payerAlias { get; set; }
        public string payeeAlias { get; set; }
        public decimal amount { get; set; }
        public string currency { get; set; }
        public string message { get; set; }
        public string status { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime? datePaid { get; set; }
        public string ErrorMessage { get; set; }
    }


    public class Misc
    {
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                input.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }
    }

    public enum SwishRequestType
    {
        CreatePaymentRequest = 0,
        GetPaymentRequest = 1,
        CancelPaymentRequest = 2
    }

    public static class URL
    {
        public static string ProductionBaseURL = "https://cpc.getswish.net/";
        public static string ProductionPaymentRequest = ProductionBaseURL + "swish-cpcapi/api/v2/paymentrequests/";
        public static string ProductionGetQRCodeByToken = "https://mpc.getswish.net/qrg-swish/api/v1/commerce";
        public static string ProductionRefundRequest = ProductionBaseURL + "swish-cpcapi/api/v2/refunds/";
        public static string ProductionPayoutRequest = ProductionBaseURL + "swish-cpcapi/api/v1/payouts";

        public static string SandboxBaseURL = "https://staging.getswish.pub.tds.tieto.com/";
        public static string SandboxPaymentRequest = SandboxBaseURL + "cpc-swish/api/v1/paymentrequests/";
        public static string SandboxGetQRCodeByToken = SandboxBaseURL + "qrg-swish/api/v1/commerce";
        public static string SandboxRefundRequest = SandboxBaseURL + "cpc-swish/api/v1/refunds/";
        public static string SandboxPayoutRequest = SandboxBaseURL + "cpc-swish/api/v1/payouts";

        public static string EmulatorBaseURL = "https://mss.cpc.getswish.net/";
        public static string EmulatorPaymentRequest = EmulatorBaseURL + "swish-cpcapi/api/v2/paymentrequests/";
        public static string EmulatorGetQRCodeByToken = "https://mpc.getswish.net/qrg-swish/api/v1/commerce";
        public static string EmulatorRefundRequest = EmulatorBaseURL + "swish-cpcapi/api/v2/refunds/";
        public static string EmulatorPayoutRequest = EmulatorBaseURL + "swish-cpcapi/api/v1/payouts";
    }
}