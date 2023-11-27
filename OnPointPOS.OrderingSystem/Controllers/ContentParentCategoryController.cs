using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class ContentParentCategoryController : ApiController
    {
        private List<ContentCategory> _mContentCategoryLists = new List<ContentCategory>();
        private ContentCategory _mContentCategoryList = new ContentCategory();

        public ContentParentCategoryController()
        {
            _mContentCategoryLists.Add(_mContentCategoryList);
        }

        public HttpResponseMessage Get(string token, string contentTemplateId)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(contentTemplateId) )
            {
                _mContentCategoryList.Status = RestStatus.ParameterError;
                _mContentCategoryList.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategoryLists));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContentCategoryList.Status = RestStatus.AuthenticationFailed;
                _mContentCategoryList.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategoryLists));
            }

            IQueryable<DB.tContentCategory> contentCategories = new DB.ContentCategoryRepository().GetActiveByContentTemplateGuidAndContentParentCategoryGuid(new Guid(contentTemplateId), Guid.Empty);
            if (!contentCategories.Any())
            {
                _mContentCategoryList.Status = RestStatus.NotExisting;
                _mContentCategoryList.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategoryLists));
            }

            if (contentCategories.FirstOrDefault().tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentCategoryList.Status = RestStatus.NotExisting;
                _mContentCategoryList.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategoryLists));
            }

            // Populate
            _mContentCategoryLists.Clear();
            foreach (DB.tContentCategory contentCategory in contentCategories)
            {
                ContentCategory mContentCategory = new ContentCategory();
                mContentCategory.ContentCategoryId = contentCategory.ContentCategoryGuid.ToString();
                mContentCategory.Name = contentCategory.Name;
                mContentCategory.Description = contentCategory.Description;
                mContentCategory.Identifier = contentCategory.Identifier;
                mContentCategory.SortOrder = contentCategory.SortOrder;

                //mContentCategory.ContentCategoryCustom =

                _mContentCategoryLists.Add(mContentCategory);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentCategoryLists));
        }


    }
}
