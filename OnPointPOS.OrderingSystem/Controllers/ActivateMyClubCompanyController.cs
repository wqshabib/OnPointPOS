using ML.Common.Handlers.Serializers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ML.Rest2.Controllers
{
    public class ActivateMyClubCompanyController : ApiController
    {
        // id = companyId
        public HttpResponseMessage Get(string secret, string id)
        {
            List<dynamic> mResults = new List<dynamic>();
            dynamic mResult = new ExpandoObject();
            mResults.Add(mResult);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                mResult.Status = RestStatus.ParameterError;
                mResult.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(id, secret))
            {
                mResult.Status = RestStatus.AuthenticationFailed;
                mResult.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }

            Guid guidCompanyGuid = new Guid(id);

            // Delete all TEST payments
            if(new DB.PaymentService().DeleteTestPaymentsByCompanyGuid(guidCompanyGuid) != DB.Repository.Status.Success)
            {
                mResult.Status = RestStatus.Illegal;
                mResult.StatusText = "Illegal (1)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }

            // Delete all orders
            if(new DB.OrderService().DeleteOrdersByCompanyGuid(guidCompanyGuid) != DB.Repository.Status.Success)
            {
                mResult.Status = RestStatus.Illegal;
                mResult.StatusText = "Illegal (2)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }

            // Set production mode
            if(new DB.ShopConfigService().SetProductionMode(guidCompanyGuid) != DB.Repository.Status.Success)
            {
                mResult.Status = RestStatus.Illegal;
                mResult.StatusText = "Illegal (3)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
            }

            // Success
            mResult.Status = RestStatus.Success;
            mResult.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mResults));
        }


    }
}
