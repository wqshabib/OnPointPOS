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
    public class ContentListController : ApiController
    {
        private List<dynamic> _dContentLists = new List<dynamic>();
        private dynamic _dContentList = new ExpandoObject();

        public ContentListController()
        {
            _dContentLists.Add(_dContentList);
        }

        public HttpResponseMessage Post(string token, int type, int listType)
        {
            if (!Request.Content.IsFormData())
            {
                _dContentList.Status = RestStatus.FormatError;
                _dContentList.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_dContentLists));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsNumeric(type) && !ML.Common.Text.IsNumeric(listType))
            {
                _dContentList.Status = RestStatus.ParameterError;
                _dContentList.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_dContentLists));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _dContentList.Status = RestStatus.AuthenticationFailed;
                _dContentList.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_dContentLists));
            }

            DB.ContentTemplateRepository.ContentTemplateType contentTemplateType = (DB.ContentTemplateRepository.ContentTemplateType)type;

            // Get Content Template
            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByCompanyGuidAndContentTemplateType(usertoken.CompanyGuid, contentTemplateType);
            if (contentTemplate == null)
            {
                _dContentList.Status = RestStatus.NotExisting;
                _dContentList.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_dContentLists));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strSearch = dic["Search"] == null ? string.Empty : dic["Search"];

            // Get Contents
            IQueryable<DB.tContent> contents = null;
            if (!string.IsNullOrEmpty(strSearch))
            {
                contents = new DB.ContentRepository().SearchContent(contentTemplate.ContentTemplateGuid, strSearch);
                contents = new DB.ContentRepository().GetByContentListType(contentTemplate.ContentTemplateGuid, (DB.ContentRepository.ContentListType)listType, contents);
            }
            else
            {
                contents = new DB.ContentRepository().GetByContentListType(contentTemplate.ContentTemplateGuid, (DB.ContentRepository.ContentListType)listType, contents);
            }

            // Populate
            _dContentLists.Clear();
            foreach(DB.tContent content in contents)
            {
                dynamic contentList = new ExpandoObject();
                contentList.contentid = content.ContentGuid.ToString();
                contentList.name = content.Name;

                _dContentLists.Add(contentList);
            }

            // Success
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_dContentLists));
        }


    }
}
