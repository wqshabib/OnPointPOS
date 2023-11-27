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
    public class CmsContentController : ApiController
    {
        private List<dynamic> _mCmsContents = new List<dynamic>();
        private dynamic _mCmsContent = new ExpandoObject();

        public CmsContentController()
        {
            _mCmsContents.Add(_mCmsContent);
        }

        public HttpResponseMessage Get(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _mCmsContent.Status = RestStatus.ParameterError;
                _mCmsContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCmsContent.Status = RestStatus.AuthenticationFailed;
                _mCmsContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            IQueryable<DB.tCmsContent> cmsContents = new DB.CmsContentRepository().GetCmsContents(usertoken.CompanyGuid);
            if (!cmsContents.Any())
            {
                _mCmsContent.Status = RestStatus.NotExisting;
                _mCmsContent.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            // Populate
            _mCmsContents.Clear();
            foreach(DB.tCmsContent cmsContent in cmsContents)
            {
                dynamic mCmsContent = new ExpandoObject();
                mCmsContent.CmsContentId = cmsContent.CmsContentGuid.ToString();
                mCmsContent.SortOrder = cmsContent.SortOrder;
                mCmsContent.Name = cmsContent.Name;
                mCmsContent.Html = cmsContent.Html;
                mCmsContent.PageId = string.Concat("p", cmsContent.PageId.ToString());

                _mCmsContents.Add(mCmsContent);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
        }

        public HttpResponseMessage Post(string token)
        {
            if (!Request.Content.IsFormData())
            {
                _mCmsContent.Status = RestStatus.FormatError;
                _mCmsContent.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            if (string.IsNullOrEmpty(token))
            {
                _mCmsContent.Status = RestStatus.ParameterError;
                _mCmsContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCmsContent.Status = RestStatus.AuthenticationFailed;
                _mCmsContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strName = string.IsNullOrEmpty(dic["Name"]) ? string.Empty : dic["Name"];
            string strHtml = string.IsNullOrEmpty(dic["Html"]) ? string.Empty : dic["Html"];
            int intSortOrder = string.IsNullOrEmpty(dic["SortOrder"]) ? 0 : Convert.ToInt32(dic["SortOrder"]);
            // Cont...

            if (string.IsNullOrEmpty(strName))
            {
                _mCmsContent.Status = RestStatus.DataMissing;
                _mCmsContent.StatusText = "Data Missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            Guid guidCmsContentGuid = Guid.NewGuid();
            if (new DB.CmsContentService().Save(usertoken.CompanyGuid, guidCmsContentGuid, strName, strHtml, intSortOrder) != DB.Repository.Status.Success)
            {
                _mCmsContent.Status = RestStatus.GenericError;
                _mCmsContent.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            // Populate
            _mCmsContent.CmsContentId = guidCmsContentGuid.ToString();
            _mCmsContent.Name = strName;
            _mCmsContent.Html = strHtml;
            _mCmsContent.SortOrder = intSortOrder;
            // Cont...

            // Success
            _mCmsContent.Status = RestStatus.Success;
            _mCmsContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
        }

        public HttpResponseMessage Put(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mCmsContent.Status = RestStatus.FormatError;
                _mCmsContent.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            if (string.IsNullOrEmpty(token) || !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mCmsContent.Status = RestStatus.ParameterError;
                _mCmsContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCmsContent.Status = RestStatus.AuthenticationFailed;
                _mCmsContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strName = string.IsNullOrEmpty(dic["Name"]) ? string.Empty : dic["Name"];
            string strHtml = string.IsNullOrEmpty(dic["Html"]) ? string.Empty : dic["Html"];
            int intSortOrder = string.IsNullOrEmpty(dic["SortOrder"]) ? 0 : Convert.ToInt32(dic["SortOrder"]);
            // Cont...

            if (string.IsNullOrEmpty(strName))
            {
                _mCmsContent.Status = RestStatus.DataMissing;
                _mCmsContent.StatusText = "Data Missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            Guid guidCmsContentGuid = new Guid(id);
            if (new DB.CmsContentService().Save(usertoken.CompanyGuid, guidCmsContentGuid, strName, strHtml, intSortOrder) != DB.Repository.Status.Success)
            {
                _mCmsContent.Status = RestStatus.GenericError;
                _mCmsContent.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            // Populate
            _mCmsContent.CmsContentId = guidCmsContentGuid.ToString();
            _mCmsContent.Name = strName;
            _mCmsContent.Html = strHtml;
            _mCmsContent.SortOrder = intSortOrder;
            // Cont...

            // Success
            _mCmsContent.Status = RestStatus.Success;
            _mCmsContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
        }

        public HttpResponseMessage Delete(string token, string id)
        {
            if (string.IsNullOrEmpty(token) || !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mCmsContent.Status = RestStatus.ParameterError;
                _mCmsContent.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCmsContent.Status = RestStatus.AuthenticationFailed;
                _mCmsContent.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            // Delete
            if(new DB.CmsContentService().Delete(new Guid(id)) != DB.Repository.Status.Success)
            {
                _mCmsContent.Status = RestStatus.GenericError;
                _mCmsContent.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
            }

            // Success
            _mCmsContent.Status = RestStatus.Success;
            _mCmsContent.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCmsContents));
        }

    }
}
