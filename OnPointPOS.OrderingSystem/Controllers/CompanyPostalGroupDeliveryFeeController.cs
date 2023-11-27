using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;

namespace ML.Rest2.Controllers
{
    public class CompanyPostalGroupDeliveryFeeController : ApiController
    {

        List<DeliveryFeeModel> _list = new List<DeliveryFeeModel>();
        DeliveryFeeModel _deliveryFee = new DeliveryFeeModel();
        public CompanyPostalGroupDeliveryFeeController()
        {
            _list.Add(_deliveryFee);
        }

        public HttpResponseMessage Get(string secret, string orderPrinterGuid, string postalGroupGuid)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(orderPrinterGuid) && !ML.Common.Text.IsGuidNotEmpty(postalGroupGuid))
            {
                _deliveryFee.Status = RestStatus.ParameterError;
                _deliveryFee.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_list));
            }
            var orderPrinterPostal = new DB.OrderPrinterPostalRepository().GetByOrderPrinterByPostalGroupGuid(Guid.Parse(orderPrinterGuid), Guid.Parse(postalGroupGuid));//we have to send PostalGroupGuid
            
            if (orderPrinterPostal != null)
            {
                _deliveryFee.OrderPrinterGuid = orderPrinterPostal.OrderPrinter.OrderPrinterGuid;
                _deliveryFee.DeliveryFee = orderPrinterPostal.DeliveryFee;
                _deliveryFee.DeliveryMinimumAmount = orderPrinterPostal.DeliveryMinimumAmount;
            }

            // Success
            _deliveryFee.Status = RestStatus.Success;
            _deliveryFee.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_list));
        }
    }
}