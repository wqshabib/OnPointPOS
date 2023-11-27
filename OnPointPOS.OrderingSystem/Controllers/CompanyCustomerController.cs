using ML.Common.Handlers.Serializers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Specialized;
using ML.Rest2.Helper;
using Models;

namespace ML.Rest2.Controllers
{
    public class CompanyCustomerController : ApiController
    {
        List<Models.Customer> _mCustomers = new List<Models.Customer>();
        Models.Customer _mCustomer = new Models.Customer();

        public CompanyCustomerController()
        {
            _mCustomers.Add(_mCustomer);
        }

        //public HttpResponseMessage Get(string token, string customerId)
        //{
        //    if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(customerId))
        //    {
        //        _mCustomer.Status = RestStatus.ParameterError;
        //        _mCustomer.StatusText = "Parameter Error";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        //    }

        //    DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
        //    if (!usertoken.Valid)
        //    {
        //        _mCustomer.Status = RestStatus.AuthenticationFailed;
        //        _mCustomer.StatusText = "Authentication Failed";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        //    }

        //    // Get Customer
        //    DB.tCustomer customer = new DB.CustomerRepository().GetByCustomerGuid(new Guid(customerId));
        //    if(customer == null)
        //    {
        //        _mCustomer.Status = RestStatus.NotExisting;
        //        _mCustomer.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        //    }
        //    if(customer.CompanyGuid != usertoken.CompanyGuid)
        //    {
        //        _mCustomer.Status = RestStatus.NotExisting;
        //        _mCustomer.StatusText = "Not Existing";
        //        return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        //    }

        //    // Populate
        //    _mCustomer.CustomerId = customer.CustomerGuid.ToString();
        //    _mCustomer.FirstName = customer.FirstName;
        //    _mCustomer.LastName = customer.LastName;
        //    _mCustomer.PhoneNo = customer.PhoneNo;
        //    // Cont...

        //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        //}

        /// <summary>
        /// Add Customer
        /// </summary>
        /// <param name="secret"></param>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// customerId = AdditionalCustomerNo (string)
        /// <returns></returns>
        public HttpResponseMessage Post(string secret, string companyId, string customerId)
        {          

            if (!Request.Content.IsFormData())
            {
                _mCustomer.Status = RestStatus.NotFormData;
                _mCustomer.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            if (string.IsNullOrEmpty(secret) && !ML.Common.Text.IsGuidNotEmpty(companyId) && string.IsNullOrEmpty(customerId))
            {
                _mCustomer.Status = RestStatus.ParameterError;
                _mCustomer.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }


            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strFirstName = string.IsNullOrEmpty(dic["FirstName"]) ? string.Empty : dic["FirstName"];
            string strLastName = string.IsNullOrEmpty(dic["LastName"]) ? string.Empty : dic["LastName"];
            string strEmail = string.IsNullOrEmpty(dic["Email"]) ? string.Empty : dic["Email"];
            string strPhoneNo = string.IsNullOrEmpty(dic["PhoneNo"]) ? string.Empty : dic["PhoneNo"];
            string strAddress = string.IsNullOrEmpty(dic["Address"]) ? string.Empty : dic["Address"];
            string strZipCode = string.IsNullOrEmpty(dic["ZipCode"]) ? string.Empty : dic["ZipCode"];
            string strCity = string.IsNullOrEmpty(dic["City"]) ? string.Empty : dic["City"];
            string strPushRegOs = string.IsNullOrEmpty(dic["PushRegOs"]) ? string.Empty : dic["PushRegOs"];
            string strPushRegId = string.IsNullOrEmpty(dic["PushRegId"]) ? string.Empty : dic["PushRegId"];

            // Populate
            _mCustomer.CustomerId = customerId;
            _mCustomer.FirstName = strFirstName;
            _mCustomer.LastName = strLastName;
            _mCustomer.Email = strEmail;
            _mCustomer.PhoneNo = strPhoneNo;
            _mCustomer.Address = strAddress;
            _mCustomer.ZipCode = strZipCode;
            _mCustomer.City = strCity;
            _mCustomer.CustomerStatus = CustomerStatus.Updated;
            // Cont...

            _mCustomer.Status = RestStatus.Success;
            _mCustomer.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        }





    }
}
