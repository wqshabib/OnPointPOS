using ML.Common.Handlers.Serializers;
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
    public class CompanyPostalController : ApiController
    {
        List<Postal> _mPostals = new List<Postal>();
        Postal _mPostal = new Postal();

        public CompanyPostalController()
        {
            _mPostals.Add(_mPostal);
        }

        // value = take
        public HttpResponseMessage Post(string secret, string companyId, int value)
        {
            if (!Request.Content.IsFormData())
            {
                _mPostal.Status = RestStatus.NotFormData;
                _mPostal.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPostals));
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && value < 100)
            {
                _mPostal.Status = RestStatus.ParameterError;
                _mPostal.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPostals));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _mPostal.Status = RestStatus.AuthenticationFailed;
                _mPostal.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPostals));
            }

            Guid guidCompanyGuid = new Guid(companyId);

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strSearch = dic["Search"];

            // Lookup
            _mPostals.Clear();
            IQueryable<DB.tPostal> postals = new DB.PostalService().Search(strSearch, value, guidCompanyGuid);

            // Populate result
            foreach (DB.tPostal postal in postals)
            {
                _mPostal = new Postal();
                _mPostal.PostalId = postal.PostalGuid.ToString();
                _mPostal.Street = postal.Street;
                _mPostal.StreetNumber = postal.StreetNumber;
                _mPostal.StreetNumberLetter = postal.StreetNumberLetter;
                _mPostal.ZipCode = postal.ZipCode;
                _mPostal.City = postal.City;

                _mPostals.Add(_mPostal);
            }
            
            // Success
            _mPostal.Status = RestStatus.Success;
            _mPostal.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPostals));
        }

        // value = ZipCode
        public HttpResponseMessage Get(string secret, string companyId, int value)
        {
            List<dynamic> dZipCodes = new List<dynamic>();
            dynamic dZipCode = new ExpandoObject();
            dZipCodes.Add(dZipCode);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsNumeric(value))
            {
                dZipCode.Status = RestStatus.ParameterError;
                dZipCode.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dZipCodes));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                dZipCode.Status = RestStatus.AuthenticationFailed;
                dZipCode.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dZipCodes));
            }

            // Lookup
            dZipCode.ZipCodeExists = new DB.PostalService().IsZipCodeAvailable(Convert.ToInt32(value), new Guid(companyId));

            // Success
            dZipCode.Status = RestStatus.Success;
            dZipCode.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dZipCodes));
        }




    }
}
