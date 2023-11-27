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
    public class ContentPublishController : ApiController
    {
        private List<Result> _mResults = new List<Result>();
        private Result _mResult = new Result();

        public ContentPublishController()
        {
            _mResults.Add(_mResult);
        }

        public HttpResponseMessage Get(string token, string id, int publish)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id) && (publish == 0 || publish == 1))
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

            DB.ContentRepository contentRepository = new DB.ContentRepository();
            DB.tContent content = contentRepository.GetContent(new Guid(id));
            if(content == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if(content.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Publish
            content.Active = publish == 1 ? true : false;

            // Save
            if (contentRepository.Save() != DB.Repository.Status.Success)
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
