using ML.Common.Handlers.Serializers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ML.Rest2.Controllers
{
    public class CompanyUserController : ApiController
    {
        public HttpResponseMessage Post(string secret, string companyId)
        {
            List<dynamic> dUsers = new List<dynamic>();
            dynamic dUser = new ExpandoObject();
            dUsers.Add(dUser);

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId))
            {
                dUser.Status = RestStatus.ParameterError;
                dUser.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dUsers));
            }

            if (!new DB.CompanyService().IsIntegrationAuthorized(companyId, secret))
            {
                dUser.Status = RestStatus.AuthenticationFailed;
                dUser.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dUsers));
            }

            // Prepare
            Guid guidCompanyGuid = new Guid(companyId);
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strUserName = string.IsNullOrEmpty(dic["UserName"]) ? string.Empty : dic["UserName"];
            string strPassword = string.IsNullOrEmpty(dic["Password"]) ? string.Empty : dic["Password"];
            string strNewPassword = string.IsNullOrEmpty(dic["NewPassword"]) ? string.Empty : dic["NewPassword"];
            string strFirstName = string.IsNullOrEmpty(dic["FirstName"]) ? string.Empty : dic["FirstName"];
            string strLastName = string.IsNullOrEmpty(dic["LastName"]) ? string.Empty : dic["LastName"];
            DB.UserRepository.Role role = string.IsNullOrEmpty(dic["Role"]) ? DB.UserRepository.Role.ShopAdmin : (DB.UserRepository.Role)Enum.Parse(typeof(DB.UserRepository.Role), dic["Role"]);
            string strEmail = string.IsNullOrEmpty(dic["Email"]) ? string.Empty : dic["Email"];

            // Validate
            if(string.IsNullOrEmpty(strUserName) && string.IsNullOrEmpty(strPassword))
            {
                dUser.Status = RestStatus.DataMissing;
                dUser.StatusText = "Data Missing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dUsers));
            }

            DB.UserService userService = new DB.UserService();
            DB.UserRepository userRepository = new DB.UserRepository();
            DB.tUser user = userRepository.GetByUserNameAndPassword(strUserName, strPassword);
            if (user != null)
            {
                if (user.CompanyGuid != guidCompanyGuid)
                {
                    dUser.Status = RestStatus.DataError;
                    dUser.StatusText = "Data Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dUsers));
                }

                // Update User
                if (userService.SaveUser(guidCompanyGuid, strUserName, strPassword, strNewPassword, strFirstName, strLastName, role, strEmail) != DB.Repository.Status.Success)
                {
                    dUser.Status = RestStatus.GenericError;
                    dUser.StatusText = "Generic Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dUsers));
                }
            }
            else
            {
                // Create User
                if (userService.SaveUser(guidCompanyGuid, strUserName, strPassword, string.Empty, strFirstName, strLastName, role, strEmail) != DB.Repository.Status.Success)
                {
                    dUser.Status = RestStatus.GenericError;
                    dUser.StatusText = "Generic Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dUsers));
                }
            }

            // Populate
            dUser.UserId = userService.UserGuid;

            // Success
            dUser.Status = RestStatus.Success;
            dUser.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(dUsers));
        }



    }
}
