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

namespace ML.Rest2.Controllers
{
    public class SubContentGroupController : ApiController
    {
        private List<SubContentGroup> _mSubContentGroups = new List<SubContentGroup>();
        private SubContentGroup _mSubContentGroup = new SubContentGroup();

        public SubContentGroupController()
        {
            _mSubContentGroups.Add(_mSubContentGroup);
        }

        /// <summary>
        /// id = SubContentGroupId or ContentTemplateId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSubContentGroup.Status = RestStatus.ParameterError;
                _mSubContentGroup.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSubContentGroup.Status = RestStatus.AuthenticationFailed;
                _mSubContentGroup.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Prepare
            IQueryable<DB.tContentSubContentGroup> contentSubContentGroups = null;

            // Check if SubContentGroups
            contentSubContentGroups = new DB.ContentSubContentGroupRepository().GetByContentTemplateGuid(new Guid(id));

            // Check if SubContentGroup
            if(!contentSubContentGroups.Any())
            {
                contentSubContentGroups = new DB.ContentSubContentGroupRepository().GetByContentSubContentGroupGuid(new Guid(id));
            }

            if (!contentSubContentGroups.Any())
            {
                _mSubContentGroup.Status = RestStatus.NotExisting;
                _mSubContentGroup.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            if (contentSubContentGroups.FirstOrDefault().tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mSubContentGroup.Status = RestStatus.NotExisting;
                _mSubContentGroup.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Populate
            _mSubContentGroups.Clear();
            foreach (DB.tContentSubContentGroup contentSubContentGroup in contentSubContentGroups)
            {
                _mSubContentGroup = new SubContentGroup();
                _mSubContentGroup.ContentSubContentGroupId = contentSubContentGroup.ContentSubContentGroupGuid.ToString();
                _mSubContentGroup.Name = contentSubContentGroup.Name;
                _mSubContentGroup.Max = contentSubContentGroup.Max;
                _mSubContentGroup.Mandatory = contentSubContentGroup.Mandatory;

                _mSubContentGroup.Status = RestStatus.Success;
                _mSubContentGroup.StatusText = "Success";

                _mSubContentGroups.Add(_mSubContentGroup);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
        }

        /// <summary>
        /// id = ContentTemplateId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Post(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mSubContentGroup.Status = RestStatus.FormatError;
                _mSubContentGroup.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSubContentGroup.Status = RestStatus.ParameterError;
                _mSubContentGroup.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }
            Guid guidContentTemplateGuid = new Guid(id);

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSubContentGroup.Status = RestStatus.AuthenticationFailed;
                _mSubContentGroup.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            DB.tContentTemplate contentTemplate = new DB.ContentTemplateRepository().GetByContentTemplateGuid(guidContentTemplateGuid);
            if (contentTemplate == null)
            {
                _mSubContentGroup.Status = RestStatus.NotExisting;
                _mSubContentGroup.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            if (contentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mSubContentGroup.Status = RestStatus.NotExisting;
                _mSubContentGroup.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Read data
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            if (dic.Count == 0)
            {
                _mSubContentGroup.Status = RestStatus.DataMissing;
                _mSubContentGroup.StatusText = "Data Missing (Name)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Prepare
            DB.ContentSubContentGroupRepository contentSubContentGroupRepository = new DB.ContentSubContentGroupRepository();
            DB.tContentSubContentGroup contentSubContentGroup = new DB.tContentSubContentGroup();
            Guid guidContentSubContentGroupGuid = Guid.NewGuid();
            contentSubContentGroup.ContentSubContentGroupGuid = guidContentSubContentGroupGuid;
            contentSubContentGroup.ContentTemplateGuid = guidContentTemplateGuid;
            if (!string.IsNullOrEmpty(dic["Name"]))
            {
                contentSubContentGroup.Name = dic["Name"];
            }
            else
            {
                _mSubContentGroup.Status = RestStatus.DataMissing;
                _mSubContentGroup.StatusText = "Data Missing (Name)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }
            if (!string.IsNullOrEmpty(dic["Max"]))
            {
                contentSubContentGroup.Max = Convert.ToInt32(dic["Max"]);
            }
            else
            {
                contentSubContentGroup.Max = 1;
            }
            if (!string.IsNullOrEmpty(dic["Mandatory"]))
            {
                contentSubContentGroup.Mandatory = Convert.ToBoolean(dic["Mandatory"]);
            }
            else
            {
                contentSubContentGroup.Mandatory = false;
            }


            // Verify Name does not exist
            if (new DB.ContentSubContentGroupRepository().GetByContentTemplateGuidAndName(guidContentTemplateGuid, contentSubContentGroup.Name) != null)
            {
                _mSubContentGroup.Status = RestStatus.AlreadyExists;
                _mSubContentGroup.StatusText = "Already Exists";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Save
            if (contentSubContentGroupRepository.Save(contentSubContentGroup) != DB.Repository.Status.Success)
            {
                _mSubContentGroup.Status = RestStatus.GenericError;
                _mSubContentGroup.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Populate result
            _mSubContentGroup.ContentSubContentGroupId = contentSubContentGroup.ContentSubContentGroupGuid.ToString();

            _mSubContentGroup.Status = RestStatus.Success;
            _mSubContentGroup.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
        }

        /// <summary>
        /// id = ContentSubContentGroupId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Put(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mSubContentGroup.Status = RestStatus.FormatError;
                _mSubContentGroup.StatusText = "Format Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSubContentGroup.Status = RestStatus.ParameterError;
                _mSubContentGroup.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }
            Guid guidContentSubContentGroupGuid = new Guid(id);

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSubContentGroup.Status = RestStatus.AuthenticationFailed;
                _mSubContentGroup.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            DB.ContentSubContentGroupRepository contentSubContentGroupRepository = new DB.ContentSubContentGroupRepository();
            DB.tContentSubContentGroup contentSubContentGroup = contentSubContentGroupRepository.GetContentSubContentGroup(guidContentSubContentGroupGuid);
            if (contentSubContentGroup == null)
            {
                _mSubContentGroup.Status = RestStatus.NotExisting;
                _mSubContentGroup.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            if (contentSubContentGroup.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mSubContentGroup.Status = RestStatus.NotExisting;
                _mSubContentGroup.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Read data
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            if (dic.Count == 0)
            {
                _mSubContentGroup.Status = RestStatus.DataMissing;
                _mSubContentGroup.StatusText = "Data Missing (Name)";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Prepare
            if (!string.IsNullOrEmpty(dic["Name"]))
            {
                contentSubContentGroup.Name = dic["Name"];
            }
            if (!string.IsNullOrEmpty(dic["Max"]))
            {
                contentSubContentGroup.Max = Convert.ToInt32(dic["Max"]);
            }
            if (!string.IsNullOrEmpty(dic["Mandatory"]))
            {
                contentSubContentGroup.Mandatory = Convert.ToBoolean(dic["Mandatory"]);
            }


            // Save
            if (contentSubContentGroupRepository.Save() != DB.Repository.Status.Success)
            {
                _mSubContentGroup.Status = RestStatus.GenericError;
                _mSubContentGroup.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Populate result
            _mSubContentGroup.ContentSubContentGroupId = contentSubContentGroup.ContentSubContentGroupGuid.ToString();

            _mSubContentGroup.Status = RestStatus.Success;
            _mSubContentGroup.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
        }

        /// <summary>
        /// id = ContentSubContentGroupId
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Delete(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mSubContentGroup.Status = RestStatus.ParameterError;
                _mSubContentGroup.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }
            Guid guidContentSubContentGroupGuid = new Guid(id);

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mSubContentGroup.Status = RestStatus.AuthenticationFailed;
                _mSubContentGroup.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            DB.tContentSubContentGroup contentSubContentGroup = new DB.ContentSubContentGroupRepository().GetContentSubContentGroup(guidContentSubContentGroupGuid);
            if (contentSubContentGroup == null)
            {
                _mSubContentGroup.Status = RestStatus.NotExisting;
                _mSubContentGroup.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            if (contentSubContentGroup.tContentTemplate.CompanyGuid != usertoken.CompanyGuid)
            {
                _mSubContentGroup.Status = RestStatus.NotExisting;
                _mSubContentGroup.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            // Delete
            if (new DB.ContentSubContentGroupService().DeleteByContentSubContentGroupGuid(guidContentSubContentGroupGuid) != DB.Repository.Status.Success)
            {
                _mSubContentGroup.Status = RestStatus.GenericError;
                _mSubContentGroup.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
            }

            _mSubContentGroup.Status = RestStatus.Success;
            _mSubContentGroup.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mSubContentGroups));
        }



    }
}
