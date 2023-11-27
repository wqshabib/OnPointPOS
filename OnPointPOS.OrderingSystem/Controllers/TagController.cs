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
    public class TagController : ApiController
    {
        private List<dynamic> _mResults = new List<dynamic>();
        private dynamic _mResult = new ExpandoObject();

        public TagController()
        {
            _mResults.Add(_mResult);
        }

        // id = tagListId
        public HttpResponseMessage Post(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mResult.Status = RestStatus.NotFormData;
                _mResult.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(id))
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

            Guid guidTagListGuid = new Guid(id);

            DB.tTagList tagList = new DB.TagListRepository().GetTagList(guidTagListGuid);
            if(tagList == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if(tagList.CompanyGuid != usertoken.CompanyGuid)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strName = string.IsNullOrEmpty(dic["Name"]) ? string.Empty : dic["Name"];
            
            if(string.IsNullOrEmpty(strName))
            {
                _mResult.Status = RestStatus.Illegal;
                _mResult.StatusText = "Illegal";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Ensure Tag is not existing
            DB.tTag tag = new DB.TagRepository().GetTag(guidTagListGuid, strName);
            if(tag != null)
            {
                _mResult.Status = RestStatus.AlreadyExists;
                _mResult.StatusText = "Already Exists";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Add
            tag = new DB.tTag();
            tag.TagGuid = Guid.NewGuid();
            tag.TagListGuid = guidTagListGuid;
            tag.Name = strName;
            DB.Repository.Status status = new DB.TagRepository().Save(tag);

            if(status != DB.Repository.Status.Success)
            {
                _mResult.Status = RestStatus.GenericError;
                _mResult.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Populate
            _mResult.TagId = tag.TagGuid.ToString();

            // Success
            _mResult.Status = RestStatus.Success;
            _mResult.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }

        // id = tagId
        public HttpResponseMessage Put(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mResult.Status = RestStatus.NotFormData;
                _mResult.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(id))
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

            Guid guidTagGuid = new Guid(id);

            DB.TagRepository tagRepository = new DB.TagRepository();
            DB.tTag tag = tagRepository.GetTag(guidTagGuid);
            if (tag == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if (tag.tTagList.CompanyGuid != usertoken.CompanyGuid)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strName = string.IsNullOrEmpty(dic["Name"]) ? string.Empty : dic["Name"];

            if (string.IsNullOrEmpty(strName))
            {
                _mResult.Status = RestStatus.Illegal;
                _mResult.StatusText = "Illegal";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Update
            tag.Name = strName;
            DB.Repository.Status status = tagRepository.Save();

            if (status != DB.Repository.Status.Success)
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

        // id = tagId
        public HttpResponseMessage Delete(string token, string id)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(id))
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

            Guid guidTagGuid = new Guid(id);

            DB.TagRepository tagRepository = new DB.TagRepository();
            DB.tTag tag = tagRepository.GetTag(guidTagGuid);
            if (tag == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if (tag.tTagList.CompanyGuid != usertoken.CompanyGuid)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Delete
            tagRepository.Delete(tag);
            DB.Repository.Status status = tagRepository.Save();

            if (status != DB.Repository.Status.Success)
            {
                _mResult.Status = RestStatus.Illegal;
                _mResult.StatusText = "Illegal";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Success
            _mResult.Status = RestStatus.Success;
            _mResult.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }


    }
}
