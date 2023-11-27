using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;

namespace ML.Rest2.Controllers
{
    public class RestaurantSalesController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">1>All, 2 > without DeliveryFee</param>
        /// <returns></returns>
        public HttpResponseMessage Get(string token, string payoutGuid, string orderPrinterGuid, int reportType)
        {
            if (string.IsNullOrEmpty(token) && ML.Common.Text.IsGuid(payoutGuid) && ML.Common.Text.IsGuid(orderPrinterGuid))
                return new HttpResponseMessage(HttpStatusCode.NotAcceptable);
            DB.tPayout payout = new DB.PayoutRepository().GetPayout(Guid.Parse(payoutGuid));
            if (payout == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
            // Try lookup printer
            IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetManyByOrderPrinterGuid(new Guid(orderPrinterGuid));
            if (!orderPrinters.Any())
            {
                orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(new Guid(orderPrinterGuid));
            }
            string restaurantTitle = string.Empty;
            
            if (orderPrinters.Any())
            {
                var orderPrinter = orderPrinters.FirstOrDefault();
                if (orderPrinter == null)
                    return new HttpResponseMessage(HttpStatusCode.NoContent);
                restaurantTitle = orderPrinter.Name;
            }
            MemoryStream ms = DB.RestaurantSaleService.CreateRestaurantSalesReport(new Guid(orderPrinterGuid), payout, restaurantTitle, reportType);

            var strFileName = string.Format("KassaRapport_{0}_{1}.pdf", restaurantTitle.Replace(" ", "_"), payout.Title.Replace(" ", "_"));
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.NoContent);

            if (ms.Length > 0)
            {
                httpResponseMessage.StatusCode = HttpStatusCode.OK;
                byte[] fileBytes = ms.ToArray();
                httpResponseMessage.Content = new ByteArrayContent(fileBytes);
                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                httpResponseMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                httpResponseMessage.Content.Headers.ContentDisposition.FileName = strFileName;
            }
            return httpResponseMessage;
        }
	}
}