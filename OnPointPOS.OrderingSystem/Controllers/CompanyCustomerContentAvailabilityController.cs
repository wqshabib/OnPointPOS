using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ML.Rest2.Controllers
{
    public class CompanyCustomerContentAvailabilityController : ApiController
    {
        private List<Result> _mResults = new List<Result>();
        private Result _mResult = new Result();

        public CompanyCustomerContentAvailabilityController()
        {
            _mResults.Add(_mResult);
        }

        public HttpResponseMessage Get(string secret, string companyId, string customerId)
        {
            if (Redirection.IsValid(companyId, 
                Request.RequestUri.ToString()
                ,"CompanyCustomerContentAvailabilityController-> public HttpResponseMessage Get(string secret, string companyId, string customerId)"
                ,@"api/CompanyCustomerContentAvailability/Get/" + secret + "/" + companyId + "/" + customerId
                )
                )
            {
                APIRedirect apiRedirect = new APIRedirect();
                string url = @"api/CompanyCustomerContentAvailability/Get/" + secret + "/" + companyId + "/" + customerId;
                return apiRedirect.GetRequst(url);
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !string.IsNullOrEmpty(customerId))
            {
                _mResult.Status = RestStatus.ParameterError;
                _mResult.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _mResult.Status = RestStatus.AuthenticationFailed;
                _mResult.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }


            Guid guidCompanyGuid = new Guid(companyId);
            DB.tCustomer customer = new DB.CustomerRepository().GetByCompanyGuidAndAdditionalCustomerNo(guidCompanyGuid, customerId);
            if(customer == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Get Content Template
            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(guidCompanyGuid, DB.ContentTemplateRepository.ContentTemplateType.Mofr);
            if (contentTemplate == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Add CustomerContentAvailability
            new DB.CustomerService().IncreaseReedemableContentForRedeemed(customer.CustomerGuid, contentTemplate.ContentTemplateGuid);

            // Success
            _mResult.Status = RestStatus.Success;
            _mResult.StatusText = "Success";

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }



        //

    }
}
