using ML.Common.Handlers.Serializers;
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
    public class ContentVariantTemplateController : ApiController
    {
        private List<ContentVariantTemplate> _mContentVariantTemplates = new List<ContentVariantTemplate>();
        private ContentVariantTemplate _mContentVariantTemplate = new ContentVariantTemplate();

        public ContentVariantTemplateController()
        {
            _mContentVariantTemplates.Add(_mContentVariantTemplate);
        }

        // Get one
        /// <summary>
        /// id = ContentVariantTemplateGuid or ContentTemplateGuid  
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContentVariantTemplate.Status = RestStatus.ParameterError;
                _mContentVariantTemplate.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if(!usertoken.Valid)
            {
                _mContentVariantTemplate.Status = RestStatus.AuthenticationFailed;
                _mContentVariantTemplate.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            // Prepare
            IQueryable<DB.tContentVariantTemplate> contentVariantTemplates = null;

            // Check if ContentVariantTemplate
            contentVariantTemplates = new DB.ContentVariantTemplateRepository().GetByContentVariantTemplateGuid(new Guid(id));

            // // Check if ContentTemplate
            if (!contentVariantTemplates.Any())
            {
                contentVariantTemplates = new DB.ContentVariantTemplateRepository().GetByContentTemplateGuid(new Guid(id));
            }

            if (!contentVariantTemplates.Any())
            {
                _mContentVariantTemplate.Status = RestStatus.NotExisting;
                _mContentVariantTemplate.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }
            if (contentVariantTemplates.FirstOrDefault().tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentVariantTemplate.Status = RestStatus.NotExisting;
                _mContentVariantTemplate.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            // Populate
            _mContentVariantTemplates.Clear();
            foreach (DB.tContentVariantTemplate contentVariantTemplate in contentVariantTemplates)
            {
                _mContentVariantTemplate = new ContentVariantTemplate();
                _mContentVariantTemplate.ContentVariantTemplateId = contentVariantTemplate.ContentVariantTemplateGuid.ToString();
                _mContentVariantTemplate.Name = contentVariantTemplate.Name;
                _mContentVariantTemplate.ContentTemplateId = contentVariantTemplate.ContentTemplateGuid.ToString();
                _mContentVariantTemplate.SortOrder = contentVariantTemplate.SortOrder;

                _mContentVariantTemplates.Add(_mContentVariantTemplate);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
        }

        // Post
        /// <summary>
        /// id = ContentTemplateGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Post(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mContentVariantTemplate.Status = RestStatus.NotFormData;
                _mContentVariantTemplate.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContentVariantTemplate.Status = RestStatus.ParameterError;
                _mContentVariantTemplate.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }
            
            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContentVariantTemplate.Status = RestStatus.AuthenticationFailed;
                _mContentVariantTemplate.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            Guid guidContentTemplateGuid = new Guid(id);
            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByContentTemplateGuid(guidContentTemplateGuid);
            if (contentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentVariantTemplate.Status = RestStatus.NotExisting;
                _mContentVariantTemplate.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            // Read data
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            // Prepare
            DB.tContentVariantTemplate contentVariantTemplate = new DB.tContentVariantTemplate();
            Guid guidContentVariantTemplateGuid = Guid.NewGuid();
            contentVariantTemplate.ContentVariantTemplateGuid = guidContentVariantTemplateGuid;
            contentVariantTemplate.ContentTemplateGuid = new Guid(id);
            if(!string.IsNullOrEmpty(dic["Name"]))
            {
                contentVariantTemplate.Name = dic["Name"];
            }
            else
            {
                _mContentVariantTemplate.Status = RestStatus.DataMissing;
                _mContentVariantTemplate.StatusText = "Data Missing (Name)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }
            if (!string.IsNullOrEmpty(dic["SortOrder"]))
            {
                contentVariantTemplate.SortOrder = Convert.ToInt32(dic["SortOrder"]);
            }
            else
            {
                contentVariantTemplate.SortOrder = 0;
            }
            contentVariantTemplate.TimeStamp = DateTime.Now;

            // Verify Name does not exist
            if(new DB.ContentVariantTemplateRepository().GetByContentTemplateGuidAndName(new Guid(id), contentVariantTemplate.Name) != null)
            {
                _mContentVariantTemplate.Status = RestStatus.AlreadyExists;
                _mContentVariantTemplate.StatusText = "Already Exists";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            // Save
            if(new DB.ContentVariantTemplateRepository().Save(contentVariantTemplate) != DB.Repository.Status.Success)
            {
                _mContentVariantTemplate.Status = RestStatus.GenericError;
                _mContentVariantTemplate.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            // Populate result
            _mContentVariantTemplate.ContentVariantTemplateId = contentVariantTemplate.ContentVariantTemplateGuid.ToString();
            _mContentVariantTemplate.ContentTemplateId = contentVariantTemplate.ContentTemplateGuid.ToString();
            _mContentVariantTemplate.Name = contentVariantTemplate.Name;
            _mContentVariantTemplate.SortOrder = contentVariantTemplate.SortOrder;

            _mContentVariantTemplate.Status = RestStatus.Success;
            _mContentVariantTemplate.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
        }

        // Put/update
        /// <summary>
        /// id = ContentVariantTemplteGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Put(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mContentVariantTemplate.Status = RestStatus.NotFormData;
                _mContentVariantTemplate.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContentVariantTemplate.Status = RestStatus.ParameterError;
                _mContentVariantTemplate.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContentVariantTemplate.Status = RestStatus.AuthenticationFailed;
                _mContentVariantTemplate.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            DB.ContentVariantTemplateRepository contentVariantTemplateRepository = new DB.ContentVariantTemplateRepository();
            DB.tContentVariantTemplate contentVariantTemplate = contentVariantTemplateRepository.GetContentVariantTemplate(new Guid(id));
            if (contentVariantTemplate == null)
            {
                _mContentVariantTemplate.Status = RestStatus.NotExisting;
                _mContentVariantTemplate.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            if (contentVariantTemplate.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentVariantTemplate.Status = RestStatus.NotExisting;
                _mContentVariantTemplate.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            // Prepare
            if (!string.IsNullOrEmpty(dic["Name"]))
            {
                contentVariantTemplate.Name = dic["Name"];
            }
            else
            {
                _mContentVariantTemplate.Status = RestStatus.DataMissing;
                _mContentVariantTemplate.StatusText = "Data Missing (Name)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }
            if (!string.IsNullOrEmpty(dic["SortOrder"]))
            {
                contentVariantTemplate.SortOrder = Convert.ToInt32(dic["SortOrder"]);
            }
            else
            {
                contentVariantTemplate.SortOrder = 0;
            }
            contentVariantTemplate.TimeStamp = DateTime.Now;

            // Verify Name does not exist
            DB.tContentVariantTemplate contentVariantTemplateCompare = new DB.ContentVariantTemplateRepository().GetByContentTemplateGuidAndName(contentVariantTemplate.tContentTemplate.ContentTemplateGuid, contentVariantTemplate.Name);
            if (contentVariantTemplateCompare != null)
            {
                if (contentVariantTemplate.ContentVariantTemplateGuid != contentVariantTemplateCompare.ContentVariantTemplateGuid)
                {
                    _mContentVariantTemplate.Status = RestStatus.AlreadyExists;
                    _mContentVariantTemplate.StatusText = "Already Exists";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
                }
            }

            // Save
            if (contentVariantTemplateRepository.Save() != DB.Repository.Status.Success)
            {
                _mContentVariantTemplate.Status = RestStatus.GenericError;
                _mContentVariantTemplate.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            // Populate result
            _mContentVariantTemplate.ContentVariantTemplateId = contentVariantTemplate.ContentVariantTemplateGuid.ToString();
            _mContentVariantTemplate.ContentTemplateId = contentVariantTemplate.ContentTemplateGuid.ToString();
            _mContentVariantTemplate.Name = contentVariantTemplate.Name;
            _mContentVariantTemplate.SortOrder = contentVariantTemplate.SortOrder;

            _mContentVariantTemplate.Status = RestStatus.Success;
            _mContentVariantTemplate.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
        }

        // Delete
        /// <summary>
        /// id = ContentVariantTemplateGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Delete(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mContentVariantTemplate.Status = RestStatus.ParameterError;
                _mContentVariantTemplate.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mContentVariantTemplate.Status = RestStatus.AuthenticationFailed;
                _mContentVariantTemplate.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            DB.ContentVariantTemplateRepository contentVariantTemplateRepository = new DB.ContentVariantTemplateRepository();
            DB.tContentVariantTemplate contentVariantTemplate = contentVariantTemplateRepository.GetContentVariantTemplate(new Guid(id));
            if (contentVariantTemplate == null)
            {
                _mContentVariantTemplate.Status = RestStatus.NotExisting;
                _mContentVariantTemplate.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            if (contentVariantTemplate.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mContentVariantTemplate.Status = RestStatus.NotExisting;
                _mContentVariantTemplate.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            // Delete related ContentVariants
            new DB.ContentVariantService().DeleteByContentVariantTemplateGuid(contentVariantTemplate.ContentVariantTemplateGuid);

            // Delete ContentVaraintTemplate
            if (new DB.ContentVariantTemplateService().DeleteByContentVariantTemplateGuid(contentVariantTemplate.ContentVariantTemplateGuid) != DB.Repository.Status.Success)
            {
                _mContentVariantTemplate.Status = RestStatus.GenericError;
                _mContentVariantTemplate.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
            }

            _mContentVariantTemplate.Status = RestStatus.Success;
            _mContentVariantTemplate.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mContentVariantTemplates));
        }


    }
}
