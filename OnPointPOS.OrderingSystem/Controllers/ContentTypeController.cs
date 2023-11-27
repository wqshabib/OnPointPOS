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
    public class ContentTypeController : ApiController
    {
        private List<ContentType> _mContentTypess = new List<ContentType>();
        private ContentType _mContentType = new ContentType();

        public ContentTypeController()
        {
            _mContentTypess.Add(_mContentType);
        }

        public HttpResponseMessage Get(string token)
        {
            List<ContentType> mContentTypes = new List<ContentType>();
            ContentType mContentType = new ContentType();

            if (string.IsNullOrEmpty(token))
            {
                mContentType.Status = RestStatus.ParameterError;
                mContentType.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mContentTypes));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                mContentType.Status = RestStatus.AuthenticationFailed;
                mContentType.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mContentTypes));
            }

            // Get Content Types
            IQueryable<DB.tContentType> contentTypes = new DB.ContentTypeRepository().GetContentTypes(usertoken.CompanyGuid);
            if (!contentTypes.Any())
            {
                mContentType.Status = RestStatus.NotExisting;
                mContentType.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mContentTypes));
            }

            // Populate
            mContentTypes.Clear();
            foreach (DB.tContentType contentType in contentTypes)
            {
                mContentType = new ContentType();
                mContentType.Name = contentType.Name;
                mContentType.ContentTypeId = contentType.ContentTypeGuid.ToString();
                mContentTypes.Add(mContentType);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(mContentTypes));
        }
    }
}
