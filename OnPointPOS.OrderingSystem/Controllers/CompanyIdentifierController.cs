using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class CompanyIdentifierController : ApiController
    {
        public HttpResponseMessage Get(string secret, string companyId)
        {
            List<dynamic> mIdentifiers = new List<dynamic>();
            dynamic dIdentifier = new ExpandoObject();
            mIdentifiers.Add(dIdentifier);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(companyId))
            {
                dIdentifier.Status = RestStatus.ParameterError;
                dIdentifier.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mIdentifiers));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                dIdentifier.Status = RestStatus.AuthenticationFailed;
                dIdentifier.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mIdentifiers));
            }

            Guid guidCompanyGuid = new Guid(companyId);

            DB.tCompany company = new DB.CompanyRepository().GetByCompanyGuid(guidCompanyGuid);
            if (company == null)
            {
                dIdentifier.Status = RestStatus.NotExisting;
                dIdentifier.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mIdentifiers));
            }

            dIdentifier.Identifier = string.IsNullOrEmpty(company.Identifier) ? string.Empty : company.Identifier;

            // Success
            dIdentifier.Status = RestStatus.Success;
            dIdentifier.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mIdentifiers));
        }
    }
}
