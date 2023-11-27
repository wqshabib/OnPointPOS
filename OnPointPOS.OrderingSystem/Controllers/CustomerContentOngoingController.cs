using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class CustomerContentOngoingController : ApiController
    {
        private List<Result> _mResults = new List<Result>();
        private Result _mResult = new Result();

        public CustomerContentOngoingController()
        {
            _mResults.Add(_mResult);
        }

        public HttpResponseMessage Get(string secret, string companyId, string customerId, string contentId)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(customerId) && !ML.Common.Text.IsGuidNotEmpty(contentId))
            {
                _mResult.Status = RestStatus.ParameterError;
                _mResult.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }
            Guid guidCompanyGuid = new Guid(companyId);

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _mResult.Status = RestStatus.AuthenticationFailed;
                _mResult.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Get Customer
            DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, customerId);
            if (customer == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            Guid guidContentGuid = new Guid(contentId);

            // Get Content
            DB.tContent content = new DB.ContentRepository().GetContent(new Guid(contentId));
            if (content == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Set Ongoing
            DB.Repository.Status status = new DB.CustomerContentOngoingService().SetOngoing(customer.CustomerGuid, guidContentGuid);
            if (status != DB.Repository.Status.Success && status != DB.Repository.Status.AlreadyExisting)
            {
                _mResult.Status = RestStatus.GenericError;
                _mResult.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Success
            _mResult.Status = RestStatus.Success;
            _mResult.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }



    }
}
