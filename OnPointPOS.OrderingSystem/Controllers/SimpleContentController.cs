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
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class SimpleContentController : ApiController
    {
        private List<SimpleContentList> _mSimpleContentLists = new List<SimpleContentList>();
        private SimpleContentList _mSimpleContentList = new SimpleContentList();
        
        public SimpleContentController()
        {
            _mSimpleContentLists.Add(_mSimpleContentList);
        }

        public HttpResponseMessage Get(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSimpleContentList.Status = RestStatus.ParameterError;
                _mSimpleContentList.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentLists));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSimpleContentList.Status = RestStatus.AuthenticationFailed;
                _mSimpleContentList.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentLists));
            }

            // Prepare
            IQueryable<DB.tContent> contents = null;

            // Check if ContentCategory
            contents = new DB.ContentRepository().GetRelevantByContentCategoryGuid(new Guid(id));

            // Check if ContentTemplate
            if (!contents.Any())
            {
                contents = new DB.ContentRepository().GetRelevantByContentTemplateGuid(new Guid(id));
            }

            if (!contents.Any())
            {
                _mSimpleContentList.Status = RestStatus.NotExisting;
                _mSimpleContentList.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentLists));
            }

            if (contents.FirstOrDefault().tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mSimpleContentList.Status = RestStatus.NotExisting;
                _mSimpleContentList.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentLists));
            }

            // Populate
            _mSimpleContentLists.Clear();
            IQueryable<DB.tContentCategory> contentCategories = new DB.ContentCategoryRepository().GetContentCategoriesByTemplateGuid(contents.FirstOrDefault().ContentTemplateGuid);
            foreach (DB.tContentCategory contentCategory in contentCategories)
            {
                _mSimpleContentList = new SimpleContentList();
                _mSimpleContentList.ContentCategoryName = contentCategory.Name;

                List<SimpleContent> mSimpleContents = new List<SimpleContent>();
                foreach (DB.tContent content in contents)
                {
                    if (content.tContentCategory.Where(c => c.ContentCategoryGuid == contentCategory.ContentCategoryGuid).Any())
                    {
                        SimpleContent simpleContent = new SimpleContent();
                        simpleContent.Name = content.Name;
                        simpleContent.Description = content.Description;
                        simpleContent.Price = content.Price;
                        mSimpleContents.Add(simpleContent);
                    }
                }

                _mSimpleContentList.SimpleContent = mSimpleContents;
                if (_mSimpleContentList.SimpleContent.Any())
                {
                    _mSimpleContentLists.Add(_mSimpleContentList);
                }
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentLists));
        }


    }
}
