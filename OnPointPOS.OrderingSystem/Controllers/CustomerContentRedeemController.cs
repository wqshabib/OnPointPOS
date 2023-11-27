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
    public class CustomerContentRedeemController : ApiController
    {
        private List<Result> _mResults = new List<Result>();
        private Result _mResult = new Result();

        public CustomerContentRedeemController()
        {
            _mResults.Add(_mResult);
        }

        public HttpResponseMessage Get(string secret, string companyId, string customerId, string contentId)
        {
            if (Redirection.IsValid(companyId,
                Request.RequestUri.ToString()
                , "CustomerContentRedeemController-> public HttpResponseMessage Get(string secret, string companyId, string customerId, string contentId)"
                , @"api/CustomerContentRedeem/Get/" + secret + "/" + companyId + "/" + customerId + "/" + contentId
                )
                )
            {
                APIRedirect apiRedirect = new APIRedirect();
                string url = @"api/CustomerContentRedeem/Get/" + secret + "/" + companyId + "/" + customerId + "/" + contentId;
                return apiRedirect.GetRequst(url);
            }

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

            // Validate Availability
            DB.tCustomerContentAvailability customerContentAvailability = new DB.CustomerContentAvailabilityRepository().GetByCustomerGuidAndContentGuid(customer.CustomerGuid, guidContentGuid);
            if (customerContentAvailability == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if(customerContentAvailability.NoOfRedeems <= 0)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Redeem
            new ML.Customer.CustomerContentAvailabilityTableAdapters.tCustomerContentAvailabilityTableAdapter().DecrementNoOfRedeems(customerContentAvailability.CustomerContentAvailabilityGuid);

            // Try Delete Ongoing
            new DB.CustomerContentOngoingService().DeleteOngoing(customer.CustomerGuid, guidContentGuid);

            // Log
            new ML.Customer.CustomerContentTableAdapters.tCustomerContentTableAdapter().InsertCustomerContent(Guid.NewGuid(), customer.CustomerGuid, guidContentGuid, (int)DB.CustomerContentRepository.CustomerContentType.Redeem);

            // Success
            _mResult.Status = RestStatus.Success;
            _mResult.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }


    }
}
