using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;
using Models;

namespace ML.Rest2.Controllers
{
    public class CustomerOrderPrinterAvailabilityForDeliveryController : ApiController
    {
        private List<CustomerOrderPrinterAvailabilityForDelivery> _mCustomerOrderPrinterAvailabilityForDeliveryList = new List<CustomerOrderPrinterAvailabilityForDelivery>();
        private CustomerOrderPrinterAvailabilityForDelivery _mCustomerOrderPrinterAvailabilityForDelivery = new CustomerOrderPrinterAvailabilityForDelivery();

        public CustomerOrderPrinterAvailabilityForDeliveryController()
        {
            _mCustomerOrderPrinterAvailabilityForDeliveryList.Add(_mCustomerOrderPrinterAvailabilityForDelivery);
        }

        ///// <summary>
        ///// id = OrderPrinterGuid or ContentCategoryGuid (Connected to an OrderPrinter)
        ///// </summary>
        ///// <param name="secret"></param>
        ///// <param name="companyId"></param>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //public HttpResponseMessage Get(string secret, string companyId, string id)
        //{
        //    return Get(secret, companyId, id, Guid.Empty.ToString());
        //}


        /// <summary>
        /// id = OrderPrinterGuid or ContentCategoryGuid (Connected to an OrderPrinter) or PostalId
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string secret, string companyId, string id)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mCustomerOrderPrinterAvailabilityForDelivery.Status = RestStatus.ParameterError;
                _mCustomerOrderPrinterAvailabilityForDelivery.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterAvailabilityForDeliveryList));
            }
            // Populate
            Dictionary<string, string> dic = null;
            dic = GetForDeliveryByOrderPrinter();
            _mCustomerOrderPrinterAvailabilityForDeliveryList.Clear();
            foreach (var item in dic)
            {
                CustomerOrderPrinterAvailabilityForDelivery customerOrderPrinterAvailabilityForDelivery = new CustomerOrderPrinterAvailabilityForDelivery();
                customerOrderPrinterAvailabilityForDelivery.Key = item.Value; //??
                customerOrderPrinterAvailabilityForDelivery.Value = item.Key; //??
                customerOrderPrinterAvailabilityForDelivery.StatusText = string.Empty;
                _mCustomerOrderPrinterAvailabilityForDeliveryList.Add(customerOrderPrinterAvailabilityForDelivery);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterAvailabilityForDeliveryList));
        }

        private Dictionary<string, string> GetForDeliveryByOrderPrinter()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Idag tidigast: 26/12 kl:11:30", "2018-12-26 11:30:00");
            dic.Add("Idag: 26/12 kl:12:00", "2018-12-26 12:00:00");
            dic.Add("Idag: 26/12 kl:12:30", "2018-12-26 12:30:00");
            dic.Add("Idag: 26/12 kl:13:00", "2018-12-26 13:00:00");
            dic.Add("Idag: 26/12 kl:13:30", "2018-12-26 13:30:00");
            dic.Add("Idag: 26/12 kl:14:00", "2018 -12-26 14:00:00");
            dic.Add("Idag: 26/12 kl:14:30", "2018 -12-26 14:30:00");
            dic.Add("Idag: 26/12 kl:15:00", "2018 -12-26 15:00:00");

            return dic;
        }

        //public HttpResponseMessage GetOld(string secret, string companyId, string id)
        //{
        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id))
        //    {
        //        _mCustomerOrderPrinterAvailabilityForDelivery.Status = RestStatus.ParameterError;
        //        _mCustomerOrderPrinterAvailabilityForDelivery.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterAvailabilityForDeliveryList));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        _mCustomerOrderPrinterAvailabilityForDelivery.Status = RestStatus.AuthenticationFailed;
        //        _mCustomerOrderPrinterAvailabilityForDelivery.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterAvailabilityForDeliveryList));
        //    }

        //    Guid guidCompanyGuid = new Guid(companyId);
        //    Guid guidId = new Guid(id);
        //    Guid guidPostalGuid = Guid.Empty;

        //    // Find OrderPrinter
        //    DB.OrderPrinterRepository orderPrinterRepository = new DB.OrderPrinterRepository();
        //    DB.tOrderPrinter orderPrinter = null;

        //    // Try get OrderPrinter by PostalGuid
        //    orderPrinter = orderPrinterRepository.GetByCompanyGuidAndPostalGuid(guidCompanyGuid, guidId);
        //    if (orderPrinter != null)
        //    {
        //        guidPostalGuid = guidId;
        //    }

        //    // Try get OrderPrinter by ContentCategoryGuid (menuid)
        //    if (orderPrinter == null)
        //    {
        //        try
        //        {
        //            orderPrinter = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidId).FirstOrDefault();
        //        }
        //        catch { }
        //    }

        //    // Try get OrderPrinter by OrderPrinterGuid
        //    if (orderPrinter == null)
        //    {
        //        orderPrinter = orderPrinterRepository.GetByOrderPrinterGuid(guidId);
        //    }

        //    if (orderPrinter == null)
        //    {
        //        _mCustomerOrderPrinterAvailabilityForDelivery.Status = RestStatus.NotExisting;
        //        _mCustomerOrderPrinterAvailabilityForDelivery.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterAvailabilityForDeliveryList));
        //    }
        //    if (orderPrinter.CompanyGuid != guidCompanyGuid)
        //    {
        //        _mCustomerOrderPrinterAvailabilityForDelivery.Status = RestStatus.NotExisting;
        //        _mCustomerOrderPrinterAvailabilityForDelivery.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterAvailabilityForDeliveryList));
        //    }

        //    // Populate
        //    Dictionary<string, string> dic = null;
        //    if (guidPostalGuid == Guid.Empty)
        //    {
        //        dic = new DB.OrderPrinterAvailabilityService().GetForTakeAwayByOrderPrinter(orderPrinter, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Regular);
        //    }
        //    else
        //    {
        //        dic = new DB.OrderPrinterAvailabilityService().GetForDeliveryByOrderPrinter(orderPrinter, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Regular);
        //    }

        //    _mCustomerOrderPrinterAvailabilityForDeliveryList.Clear();
        //    foreach (var item in dic)
        //    {
        //        CustomerOrderPrinterAvailabilityForDelivery customerOrderPrinterAvailabilityForDelivery = new CustomerOrderPrinterAvailabilityForDelivery();
        //        customerOrderPrinterAvailabilityForDelivery.Key = item.Value; //??
        //        customerOrderPrinterAvailabilityForDelivery.Value = item.Key; //??
        //        customerOrderPrinterAvailabilityForDelivery.StatusText = string.Empty;
        //        _mCustomerOrderPrinterAvailabilityForDeliveryList.Add(customerOrderPrinterAvailabilityForDelivery);
        //    }

        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterAvailabilityForDeliveryList));
        //}


    }
}
