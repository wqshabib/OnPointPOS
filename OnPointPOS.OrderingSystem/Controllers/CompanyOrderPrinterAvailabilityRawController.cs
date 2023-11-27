using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    [RoutePrefix("api/CompanyOrderPrinterAvailabilityRaw")]
    public class CompanyOrderPrinterAvailabilityRawController : ApiController
    {
        /// <summary>
        /// id = ContentCategoryGuid (Connected to an OrderPrinter) or OrderPrinterGuid
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>

        public HttpResponseMessage Get(string secret, string companyId, string id)
        {
            List<dynamic> mResults = new List<dynamic>();
            dynamic mResult = new ExpandoObject();
            
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                mResult.Status = RestStatus.ParameterError;
                mResult.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }


            Guid guidOrderPrinterGuid = new Guid(id);


            // Populate 
            var strng = @"{[{'OrderPrinterAvailabilityList': ['Mån: 08:00 - 21:00','Tis - Sön: 08:00 - 20:00'],'OrderPrinterAvailabilityLunchList': [],'Status': 0,'StatusText': 'Success'}]}";
            var json = strng.Replace("'","\"");
            // return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(json));

            List<string> lst = new List<string>();
            lst.Add("Mån: 08:00 - 21:00");
            lst.Add("Tis - Sön: 08:00 - 20:00");
            

            mResult.OrderPrinterAvailabilityList = lst;
            mResult.OrderPrinterAvailabilityLunchList = new List<string>();

            mResult.Status = RestStatus.Success;
            mResult.StatusText = "Success";
            mResults.Add(mResult);
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
        }


        //public HttpResponseMessage GetOLD(string secret, string companyId, string id)
        //{
        //    List<dynamic> mResults = new List<dynamic>();
        //    dynamic mResult = new ExpandoObject();
        //    mResults.Add(mResult);

        //    if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(id))
        //    {
        //        mResult.Status = RestStatus.ParameterError;
        //        mResult.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
        //    }

        //    if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
        //    {
        //        mResult.Status = RestStatus.AuthenticationFailed;
        //        mResult.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
        //    }

        //    Guid guidOrderPrinterGuid = new Guid(id);
        //    IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetManyByOrderPrinterGuid(guidOrderPrinterGuid);
        //    if (!orderPrinters.Any())
        //    {
        //        Guid guidContentCategoryGuid = new Guid(id);
        //        orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidContentCategoryGuid);
        //        if (!orderPrinters.Any())
        //        {
        //            mResult.Status = RestStatus.NotExisting;
        //            mResult.StatusText = "Not Existing";
        //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
        //        }
        //        if (orderPrinters.FirstOrDefault().CompanyGuid != new Guid(companyId))
        //        {
        //            mResult.Status = RestStatus.NotExisting;
        //            mResult.StatusText = "Not Existing";
        //            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
        //        }
        //    }

        //    // Populate
        //    if (orderPrinters.Count() == 1)
        //    {
        //        mResult.OrderPrinterAvailabilityList = new DB.OrderPrinterAvailabilityService().GetListByOrderPrinterGuid(orderPrinters.FirstOrDefault().OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Regular, false);
        //        mResult.OrderPrinterAvailabilityLunchList = new DB.OrderPrinterAvailabilityService().GetListByOrderPrinterGuid(orderPrinters.FirstOrDefault().OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Lunch, false);
        //        mResult.Status = RestStatus.Success;
        //        mResult.StatusText = "Success";
        //    }
        //    else if (orderPrinters.Any())
        //    {
        //        mResults.Clear();
        //        foreach (DB.tOrderPrinter orderPrinter in orderPrinters)
        //        {
        //            mResult = new ExpandoObject();
        //            mResult.OrderPrinterAvailabilityList = new DB.OrderPrinterAvailabilityService().GetListByOrderPrinterGuid(orderPrinter.OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Regular, true);
        //            mResult.OrderPrinterAvailabilityLunchList = new DB.OrderPrinterAvailabilityService().GetListByOrderPrinterGuid(orderPrinters.FirstOrDefault().OrderPrinterGuid, DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Lunch, false);
        //            mResult.Status = RestStatus.Success;
        //            mResult.StatusText = "Success";

        //            mResults.Add(mResult);
        //        }
        //    }
        //    else
        //    {
        //        mResult.Status = RestStatus.NotExisting;
        //        mResult.StatusText = "Not Existing";
        //    }

        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
        //}




    }


}
