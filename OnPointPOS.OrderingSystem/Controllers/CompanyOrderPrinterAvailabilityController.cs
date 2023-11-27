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
using System.Text;

namespace ML.Rest2.Controllers
{
    public class CompanyOrderPrinterAvailabilityController : ApiController
    {
        private List<CompanyOrderPrinterAvailabilityHtml> _mCompanyOrderPrinterAvailabilityList = new List<CompanyOrderPrinterAvailabilityHtml>();
        private CompanyOrderPrinterAvailabilityHtml _mCompanyOrderPrinterAvailability = new CompanyOrderPrinterAvailabilityHtml();

        public CompanyOrderPrinterAvailabilityController()
        {
            _mCompanyOrderPrinterAvailabilityList.Add(_mCompanyOrderPrinterAvailability);
        }

        /// <summary>
        /// id = ContentCategoryGuid (Connected to an OrderPrinter) or OrderPrinterGuid
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
         
        //public HttpResponseMessage Get(string secret, string companyId, string id)
        //{
        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id))
        //    {
        //        _mCompanyOrderPrinterAvailability.Status = RestStatus.ParameterError;
        //        _mCompanyOrderPrinterAvailability.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinterAvailabilityList));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        _mCompanyOrderPrinterAvailability.Status = RestStatus.AuthenticationFailed;
        //        _mCompanyOrderPrinterAvailability.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinterAvailabilityList));
        //    }

        //    Guid guidOrderPrinterGuid = new Guid(id);
        //    IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetManyByOrderPrinterGuid(guidOrderPrinterGuid);
        //    if (!orderPrinters.Any())
        //    {
        //        Guid guidContentCategoryGuid = new Guid(id);
        //        orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidContentCategoryGuid);
        //        if (!orderPrinters.Any())
        //        {
        //            _mCompanyOrderPrinterAvailability.Status = RestStatus.NotExisting;
        //            _mCompanyOrderPrinterAvailability.StatusText = "Not Existing";
        //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinterAvailabilityList));
        //        }
        //        if (orderPrinters.FirstOrDefault().CompanyGuid != new Guid(companyId))
        //        {
        //            _mCompanyOrderPrinterAvailability.Status = RestStatus.NotExisting;
        //            _mCompanyOrderPrinterAvailability.StatusText = "Not Existing";
        //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinterAvailabilityList));
        //        }
        //    }

        //    //Populate
        //    if (orderPrinters.Count() == 1)
        //        if (orderPrinters.Any())
        //        {
        //            _mCompanyOrderPrinterAvailability.OrderPrinterAvailabilityList = new DB.OrderPrinterAvailabilityService().GetHtmlListByOrderPrinterGuid(orderPrinters.FirstOrDefault().OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Regular, false);
        //            _mCompanyOrderPrinterAvailability.OrderPrinterAvailabilityLunchList = new DB.OrderPrinterAvailabilityService().GetHtmlListByOrderPrinterGuid(orderPrinters.FirstOrDefault().OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Lunch, false);
        //            _mCompanyOrderPrinterAvailability.OrderPrinterAvailabilityALaCarteList = new DB.OrderPrinterAvailabilityService().GetHtmlListByOrderPrinterGuid(orderPrinters.FirstOrDefault().OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.ALaCarte, false);
        //            _mCompanyOrderPrinterAvailability.Status = RestStatus.Success;
        //            _mCompanyOrderPrinterAvailability.StatusText = "Success";
        //        }
        //        else if (orderPrinters.Any())
        //        {
        //            _mCompanyOrderPrinterAvailabilityList.Clear();
        //            foreach (DB.tOrderPrinter orderPrinter in orderPrinters)
        //            {
        //                _mCompanyOrderPrinterAvailability = new CompanyOrderPrinterAvailabilityHtml();
        //                _mCompanyOrderPrinterAvailability.OrderPrinterAvailabilityList = new DB.OrderPrinterAvailabilityService().GetHtmlListByOrderPrinterGuid(orderPrinter.OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Regular, false);
        //                _mCompanyOrderPrinterAvailability.OrderPrinterAvailabilityLunchList = new DB.OrderPrinterAvailabilityService().GetHtmlListByOrderPrinterGuid(orderPrinter.OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Lunch, false);
        //                _mCompanyOrderPrinterAvailability.OrderPrinterAvailabilityALaCarteList = new DB.OrderPrinterAvailabilityService().GetHtmlListByOrderPrinterGuid(orderPrinter.OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.ALaCarte, false);
        //                _mCompanyOrderPrinterAvailability.Name = orderPrinter.Name;
        //                _mCompanyOrderPrinterAvailability.Status = RestStatus.Success;
        //                _mCompanyOrderPrinterAvailability.StatusText = "Success";

        //                _mCompanyOrderPrinterAvailabilityList.Add(_mCompanyOrderPrinterAvailability);
        //            }
        //        }
        //        else
        //        {
        //            _mCompanyOrderPrinterAvailability.Status = RestStatus.NotExisting;
        //            _mCompanyOrderPrinterAvailability.StatusText = "Not Existing";
        //        }

        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCompanyOrderPrinterAvailabilityList));
        //}


    }
}
