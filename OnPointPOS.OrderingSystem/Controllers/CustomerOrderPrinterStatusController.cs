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
    public class CustomerOrderPrinterStatusController : ApiController
    {
        private List<CustomerOrderPrinterStatus> _mCustomerOrderPrinterStatusList = new List<CustomerOrderPrinterStatus>();
        private CustomerOrderPrinterStatus _mCustomerOrderPrinterStatus = new CustomerOrderPrinterStatus();

        public CustomerOrderPrinterStatusController()
        {
            _mCustomerOrderPrinterStatusList.Add(_mCustomerOrderPrinterStatus);
        }

        /// <summary>
        /// id = ContentCategoryGuid (Connected to an OrderPrinter) or PostalId
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string secret, string companyId, string id)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mCustomerOrderPrinterStatus.Status = RestStatus.ParameterError;
                _mCustomerOrderPrinterStatus.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
            }

            Guid guidCompanyGuid = new Guid(companyId);
            Guid guidId = new Guid(id);

            Guid guidOrderPrinterGuid = Guid.Parse("c9eb3703-25d9-4ea9-aa0c-8c7160669934");
            Guid guidPostalGuid = Guid.Empty;

            // Populate
            _mCustomerOrderPrinterStatus.OrderPrinterStatus = 2;
            _mCustomerOrderPrinterStatus.OrderPrinterStatusText = string.Empty;
            _mCustomerOrderPrinterStatus.OrderPrinterId = guidOrderPrinterGuid.ToString();
            _mCustomerOrderPrinterStatus.OrderPrinterName = "Template";


            _mCustomerOrderPrinterStatus.Status = RestStatus.Success;
            _mCustomerOrderPrinterStatus.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        }


        /// <summary>
        /// id = ContentCategoryGuid (Connected to an OrderPrinter) or PostalId
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        //public HttpResponseMessage Get(string secret, string companyId, string id, string customerId)
        //{
        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id) && customerId == string.Empty)
        //    {
        //        _mCustomerOrderPrinterStatus.Status = RestStatus.ParameterError;
        //        _mCustomerOrderPrinterStatus.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        _mCustomerOrderPrinterStatus.Status = RestStatus.AuthenticationFailed;
        //        _mCustomerOrderPrinterStatus.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        //    }

        //    Guid guidCompanyGuid = new Guid(companyId);
        //    Guid guidId = new Guid(id);

        //    Guid guidOrderPrinterGuid = Guid.Empty;
        //    Guid guidPostalGuid = Guid.Empty;

        //    IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidId);
        //    if (orderPrinters.Any())
        //    {
        //        guidOrderPrinterGuid = orderPrinters.FirstOrDefault().OrderPrinterGuid;
        //    }

        //    if (guidOrderPrinterGuid == Guid.Empty)
        //    {
        //        DB.tOrderPrinter orderPrinter = new DB.OrderPrinterRepository().GetByCompanyGuidAndPostalGuid(guidCompanyGuid, guidId);
        //        if(orderPrinter != null)
        //        {
        //            guidOrderPrinterGuid = orderPrinter.OrderPrinterGuid;
        //            guidPostalGuid = guidId;
        //        }
        //    }

        //    if (guidOrderPrinterGuid == Guid.Empty)
        //    {
        //        _mCustomerOrderPrinterStatus.Status = RestStatus.NotExisting;
        //        _mCustomerOrderPrinterStatus.StatusText = "Not Existing 1";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        //    }

        //    // Try additional customer no
        //    DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, customerId);
        //    if(customer == null)
        //    {
        //        // Try CustomerGuid
        //        if(!ML.Common.Text.IsGuidNotEmpty(customerId))
        //        {
        //            _mCustomerOrderPrinterStatus.Status = RestStatus.NotExisting;
        //            _mCustomerOrderPrinterStatus.StatusText = "Not Existing 2";
        //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        //        }

        //        customer = new DB.CustomerRepository().GetByCustomerGuid(new Guid(customerId));
        //    }

        //    decimal decAmount = 0;

        //    if (orderPrinters.Count() == 1)
        //    {
        //        DB.OrderPrinterRepository.OrderPrinterStatus orderPrinterStatus = DB.OrderPrinterService.GetAvailableStatus(guidOrderPrinterGuid, decAmount); //TODO PriceType!!!
        //        var orderPrinter = orderPrinters.FirstOrDefault();
        //        // Populate
        //        _mCustomerOrderPrinterStatus.OrderPrinterStatus = (int)orderPrinterStatus;
        //        _mCustomerOrderPrinterStatus.OrderPrinterStatusText = orderPrinterStatus.ToString();
        //        _mCustomerOrderPrinterStatus.OrderPrinterId = orderPrinter.OrderPrinterGuid.ToString();
        //        _mCustomerOrderPrinterStatus.OrderPrinterName = orderPrinter.Name;
        //    }
        //    else
        //    {
        //        _mCustomerOrderPrinterStatusList.Clear();
        //        foreach(var orderPrinter in orderPrinters)
        //        {
        //            DB.OrderPrinterRepository.OrderPrinterStatus orderPrinterStatus = DB.OrderPrinterService.GetAvailableStatus(orderPrinter.OrderPrinterGuid, decAmount); //TODO PriceType!!!
        //            _mCustomerOrderPrinterStatus = new CustomerOrderPrinterStatus();
        //            _mCustomerOrderPrinterStatus.OrderPrinterStatus = (int)orderPrinterStatus;
        //            _mCustomerOrderPrinterStatus.OrderPrinterStatusText = orderPrinterStatus.ToString();
        //            _mCustomerOrderPrinterStatus.OrderPrinterId = orderPrinter.OrderPrinterGuid.ToString();
        //            _mCustomerOrderPrinterStatus.OrderPrinterName = orderPrinter.Name;
        //            _mCustomerOrderPrinterStatusList.Add(_mCustomerOrderPrinterStatus);
        //        }
        //    }

        //    _mCustomerOrderPrinterStatus.Status = RestStatus.Success;
        //    _mCustomerOrderPrinterStatus.StatusText = "Success";
        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        //}

        //public HttpResponseMessage Get(string secret, string companyId, string id)
        //{
        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id))
        //    {
        //        _mCustomerOrderPrinterStatus.Status = RestStatus.ParameterError;
        //        _mCustomerOrderPrinterStatus.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        _mCustomerOrderPrinterStatus.Status = RestStatus.AuthenticationFailed;
        //        _mCustomerOrderPrinterStatus.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        //    }

        //    Guid guidCompanyGuid = new Guid(companyId);
        //    Guid guidId = new Guid(id);

        //    Guid guidOrderPrinterGuid = Guid.Empty;
        //    Guid guidPostalGuid = Guid.Empty;

        //    IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidId);
        //    if (orderPrinters.Any())
        //    {
        //        guidOrderPrinterGuid = orderPrinters.FirstOrDefault().OrderPrinterGuid;
        //    }

        //    if (guidOrderPrinterGuid == Guid.Empty)
        //    {
        //        DB.tOrderPrinter orderPrinter = new DB.OrderPrinterRepository().GetByCompanyGuidAndPostalGuid(guidCompanyGuid, guidId);
        //        if (orderPrinter != null)
        //        {
        //            guidOrderPrinterGuid = orderPrinter.OrderPrinterGuid;
        //            guidPostalGuid = guidId;
        //        }
        //    }

        //    if (guidOrderPrinterGuid == Guid.Empty)
        //    {
        //        _mCustomerOrderPrinterStatus.Status = RestStatus.NotExisting;
        //        _mCustomerOrderPrinterStatus.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        //    }

        //    //// Populate
        //    //_mCustomerOrderPrinterStatus.OrderPrinterStatus = DB.OrderPrinterService.IsAliveAndAvailableByOrderPrinterGuid(guidOrderPrinterGuid) ? 0 : 2;
        //    //_mCustomerOrderPrinterStatus.OrderPrinterStatusText = string.Empty;

        //    if (orderPrinters.Count() == 1)
        //    {
        //        var orderPrinter = orderPrinters.FirstOrDefault();
        //        // Populate
        //        _mCustomerOrderPrinterStatus.OrderPrinterStatus = DB.OrderPrinterService.IsAliveAndAvailableByOrderPrinterGuid(guidOrderPrinterGuid) ? 0 : 2; ;
        //        _mCustomerOrderPrinterStatus.OrderPrinterStatusText = string.Empty;
        //        _mCustomerOrderPrinterStatus.OrderPrinterId = orderPrinter.OrderPrinterGuid.ToString();
        //        _mCustomerOrderPrinterStatus.OrderPrinterName = orderPrinter.Name;
        //    }
        //    else
        //    {
        //        _mCustomerOrderPrinterStatusList.Clear();
        //        foreach (var orderPrinter in orderPrinters)
        //        {
        //            _mCustomerOrderPrinterStatus = new CustomerOrderPrinterStatus();
        //            _mCustomerOrderPrinterStatus.OrderPrinterStatus = DB.OrderPrinterService.IsAliveAndAvailableByOrderPrinterGuid(guidOrderPrinterGuid) ? 0 : 2;
        //            _mCustomerOrderPrinterStatus.OrderPrinterStatusText = string.Empty;
        //            _mCustomerOrderPrinterStatus.OrderPrinterId = orderPrinter.OrderPrinterGuid.ToString();
        //            _mCustomerOrderPrinterStatus.OrderPrinterName = orderPrinter.Name;
        //            _mCustomerOrderPrinterStatusList.Add(_mCustomerOrderPrinterStatus);
        //        }
        //    }

        //    _mCustomerOrderPrinterStatus.Status = RestStatus.Success;
        //    _mCustomerOrderPrinterStatus.StatusText = "Success";
        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomerOrderPrinterStatusList));
        //}


    }
}
