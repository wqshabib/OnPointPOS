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
    public class CompanySimpleContentParentCategoryController : ApiController
    {
        private List<SimpleContentCategory> _mSimpleContentCategoryLists = new List<SimpleContentCategory>();
        private SimpleContentCategory _mSimpleContentCategoryList = new SimpleContentCategory();

        public CompanySimpleContentParentCategoryController()
        {
            _mSimpleContentCategoryLists.Add(_mSimpleContentCategoryList);
        }

        public HttpResponseMessage Get(string secret, string companyId, string contentTemplateId)
        {
            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && !ML.Common.Text.IsGuidNotEmpty(contentTemplateId))
            {
                _mSimpleContentCategoryList.Status = RestStatus.ParameterError;
                _mSimpleContentCategoryList.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentCategoryLists));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                _mSimpleContentCategoryList.Status = RestStatus.AuthenticationFailed;
                _mSimpleContentCategoryList.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentCategoryLists));
            }

            IQueryable<DB.tContentCategory> contentCategories = new DB.ContentCategoryRepository().GetActiveByContentTemplateGuidAndContentParentCategoryGuid(new Guid(contentTemplateId), Guid.Empty);
            if (!contentCategories.Any())
            {
                _mSimpleContentCategoryList.Status = RestStatus.NotExisting;
                _mSimpleContentCategoryList.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentCategoryLists));
            }

            if (contentCategories.FirstOrDefault().tContentTemplate.CompanyGuid != new Guid(companyId))
            {
                _mSimpleContentCategoryList.Status = RestStatus.NotExisting;
                _mSimpleContentCategoryList.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentCategoryLists));
            }

            // Populate
            _mSimpleContentCategoryLists.Clear();
            foreach (DB.tContentCategory contentCategory in contentCategories)
            {
                SimpleContentCategory simpleContentCategory = new SimpleContentCategory();
                simpleContentCategory.ContentCategoryId = contentCategory.ContentCategoryGuid.ToString();
                simpleContentCategory.Name = contentCategory.Name;
                simpleContentCategory.Description = contentCategory.Description;
                _mSimpleContentCategoryLists.Add(simpleContentCategory);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSimpleContentCategoryLists));
        }


    }
}
