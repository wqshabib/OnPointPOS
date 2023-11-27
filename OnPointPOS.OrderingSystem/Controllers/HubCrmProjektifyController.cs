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
    public class HubCrmProjektifyController : ApiController
    {
        List<dynamic> _mModels = new List<dynamic>();
        dynamic _dModel = new ExpandoObject();

        public HubCrmProjektifyController()
        {
            _mModels.Add(_dModel);
        }

        [Route("api/HubCrmProjektify/CompanyToProject/{secret}")]
        [HttpPost]
        public HttpResponseMessage CompanyToProject(string secret)
        {
            if (!Request.Content.IsFormData())
            {
                _dModel.Status = RestStatus.NotFormData;
                _dModel.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            DB.HubService.Hub hub = DB.HubService.Authorize(secret);
            if (hub == null)
            {
                _dModel.Status = RestStatus.AuthenticationFailed;
                _dModel.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            string strName = string.IsNullOrEmpty(dic["Name"]) ? string.Empty : dic["Name"];
            Guid guidProjectGuid = ML.Common.Text.IsGuidNotEmpty(dic["ProjectGuid"]) ? new Guid(dic["ProjectGuid"]) : Guid.Empty;
            string strAddress = string.IsNullOrEmpty(dic["Address"]) ? string.Empty : dic["Address"];
            string strCity = string.IsNullOrEmpty(dic["City"]) ? string.Empty : dic["City"];

            // Validate
            if (guidProjectGuid == Guid.Empty)
            {
                _dModel.Status = RestStatus.ParameterError;
                _dModel.StatusText = "ParameterError - ProjectGuid";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            if (string.IsNullOrEmpty(strName))
            {
                _dModel.Status = RestStatus.ParameterError;
                _dModel.StatusText = "ParameterError - Name";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            // Save
            if (new PFY.ProjectService().Save(hub.DestinationGuid, guidProjectGuid, strName, strAddress, strCity) != PFY.Repository.Status.Success)
            {
                _dModel.Status = RestStatus.GenericError;
                _dModel.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            // Success
            _dModel.Status = RestStatus.Success;
            _dModel.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
        }


        [Route("api/HubCrmProjektify/PersonToUser/{secret}")]
        [HttpPost]
        public HttpResponseMessage PersonToUser(string secret)
        {
            if (!Request.Content.IsFormData())
            {
                _dModel.Status = RestStatus.NotFormData;
                _dModel.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            DB.HubService.Hub hub = DB.HubService.Authorize(secret);
            if (hub == null)
            {
                _dModel.Status = RestStatus.AuthenticationFailed;
                _dModel.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            Guid guidUserGuid = ML.Common.Text.IsGuidNotEmpty(dic["UserGuid"]) ? new Guid(dic["UserGuid"]) : Guid.Empty;
            string strPhoneNo = string.IsNullOrEmpty(dic["PhoneNo"]) ? string.Empty : dic["PhoneNo"];
            string strFirstName = string.IsNullOrEmpty(dic["FirstName"]) ? string.Empty : dic["FirstName"];
            string strLastName = string.IsNullOrEmpty(dic["LastName"]) ? string.Empty : dic["LastName"];
            string strCompanyName = string.IsNullOrEmpty(dic["CompanyName"]) ? string.Empty : dic["CompanyName"];
            string strEmail = string.IsNullOrEmpty(dic["Email"]) ? string.Empty : dic["Email"];

            // Validate
            if (guidUserGuid == Guid.Empty)
            {
                _dModel.Status = RestStatus.ParameterError;
                _dModel.StatusText = "ParameterError - UserGuid";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            if (string.IsNullOrEmpty(strPhoneNo))
            {
                _dModel.Status = RestStatus.ParameterError;
                _dModel.StatusText = "ParameterError - PhoneNo";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            if (string.IsNullOrEmpty(strFirstName))
            {
                _dModel.Status = RestStatus.ParameterError;
                _dModel.StatusText = "ParameterError - FirstName";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            if (string.IsNullOrEmpty(strLastName))
            {
                _dModel.Status = RestStatus.ParameterError;
                _dModel.StatusText = "ParameterError - LastName";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            // Save
            if (new PFY.UserService().Save(hub.DestinationGuid, guidUserGuid, strPhoneNo, strFirstName, strLastName, strCompanyName, strEmail) != PFY.Repository.Status.Success)
            {
                _dModel.Status = RestStatus.GenericError;
                _dModel.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
            }

            // Success
            _dModel.Status = RestStatus.Success;
            _dModel.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mModels));
        }

    }
}
