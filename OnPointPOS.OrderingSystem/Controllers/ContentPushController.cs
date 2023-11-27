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
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class ContentPushController : ApiController
    {
        private List<Result> _mResults = new List<Result>();
        private Result _mResult = new Result();

        public ContentPushController()
        {
            _mResults.Add(_mResult);
        }

        public HttpResponseMessage Post(string token, string contentId)
        {
            if (!Request.Content.IsFormData())
            {
                _mResult.Status = RestStatus.FormatError;
                _mResult.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(contentId))
            {
                _mResult.Status = RestStatus.ParameterError;
                _mResult.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mResult.Status = RestStatus.AuthenticationFailed;
                _mResult.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            Guid guidContentGuid = new Guid(contentId);

            // Ensure Content exists
            DB.tContent content = new DB.ContentRepository().GetContent(guidContentGuid);
            if(content == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Ensure Content belongs to Company
            if(content.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            DateTime dtPushDateTime = dic["PushDateTime"] == null ? DateTime.Now : Convert.ToDateTime(dic["PushDateTime"]);
            string strMessage = string.IsNullOrEmpty(dic["Message"]) ? string.Empty : dic["Message"];
           
            // Add Content Push
            if (new DB.ContentPushService().AddContentPush(guidContentGuid, strMessage, dtPushDateTime) != DB.Repository.Status.Success)
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
