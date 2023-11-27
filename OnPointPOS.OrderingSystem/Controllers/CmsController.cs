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
    public class CmsController : ApiController
    {
        private List<dynamic> _mCmss = new List<dynamic>();
        private dynamic _mCms = new ExpandoObject();

        public CmsController()
        {
            _mCmss.Add(_mCms);
        }

        public HttpResponseMessage Get(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _mCms.Status = RestStatus.ParameterError;
                _mCms.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCms.Status = RestStatus.AuthenticationFailed;
                _mCms.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
            }

            DB.tCms cms = new DB.CmsRepository().GetCms(usertoken.CompanyGuid);
            if(cms == null)
            {
                _mCms.Status = RestStatus.NotExisting;
                _mCms.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
            }

            // Populate
            _mCms.Name = cms.Name;
            // Cont...
            
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
        }

        public HttpResponseMessage Post(string token)
        {
            if (!Request.Content.IsFormData())
            {
                _mCms.Status = RestStatus.FormatError;
                _mCms.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
            }

            if (string.IsNullOrEmpty(token))
            {
                _mCms.Status = RestStatus.ParameterError;
                _mCms.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCms.Status = RestStatus.AuthenticationFailed;
                _mCms.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strName = string.IsNullOrEmpty(dic["Name"]) ? string.Empty : dic["Name"];
            // Cont...

            if(string.IsNullOrEmpty(strName))
            {
                _mCms.Status = RestStatus.DataMissing;
                _mCms.StatusText = "Data Missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
            }

            if (new DB.CmsService().Save(usertoken.CompanyGuid, strName) != DB.Repository.Status.Success)
            {
                _mCms.Status = RestStatus.GenericError;
                _mCms.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
            }

            // Populate
            _mCms.Name = strName;
            // Cont...

            // Success
            _mCms.Status = RestStatus.Success;
            _mCms.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmss));
        }




    }
}
