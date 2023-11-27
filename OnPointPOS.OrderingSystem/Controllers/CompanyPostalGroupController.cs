using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;

namespace ML.Rest2.Controllers
{    
    public class CompanyPostalGroupController : ApiController
    {
        List<Postal> _mPostals = new List<Postal>();
        Postal _mPostal = new Postal();
        public CompanyPostalGroupController()
        {
            _mPostals.Add(_mPostal);
        }

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

            var postals = new DB.PostalService().SearchByPostalGroup(strSearch, value, guidCompanyGuid);
            _mPostals = (from p in postals
                       select new Postal { PostalGroupGuid = p.PostalGroupGuid ?? Guid.Empty, Street = p.Street, ZipCode = p.ZipCode, City = p.City }).Distinct().OrderBy(p => p.Street).Take(value).ToList();

            // Success
            _mPostal.Status = RestStatus.Success;
            _mPostal.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mPostals));
        }
    }
}