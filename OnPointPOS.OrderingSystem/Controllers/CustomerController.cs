using ML.Common.Handlers.Serializers;
using ML.Rest2.Models;
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

namespace ML.Rest2.Controllers
{
    public class CustomerController : ApiController
    {
        List<Models.Customer> _mCustomers = new List<Models.Customer>();
        Models.Customer _mCustomer = new Models.Customer();

        public CustomerController()
        {
            _mCustomers.Add(_mCustomer);
        }

        public HttpResponseMessage Post(string token)
        {
            if (!Request.Content.IsFormData())
            {
                _mCustomer.Status = RestStatus.NotFormData;
                _mCustomer.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            if (string.IsNullOrEmpty(token))
            {
                _mCustomer.Status = RestStatus.ParameterError;
                _mCustomer.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCustomer.Status = RestStatus.AuthenticationFailed;
                _mCustomer.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strSearch = string.IsNullOrEmpty(dic["Search"]) ? string.Empty : dic["Search"];

            // Search Customers
            IQueryable<DB.tCustomer> customers = null;

            string[] strWords = strSearch.Split(Convert.ToChar(" "));
            string strPhoneNo = string.Empty;
            string strName1 = string.Empty;
            string strName2 = string.Empty;

            // Handle Search text
            for (int i = 0; i < strWords.Length; i++)
            {
                if (ML.Common.Text.IsNumeric(ML.Common.SmsHelper.CleanNumber(strWords[i])))
                {
                    strPhoneNo = ML.Common.SmsHelper.CleanPhoneNumber(strWords[i]);
                    if (string.IsNullOrEmpty(strPhoneNo))
                    {
                        strPhoneNo = strWords[i];
                    }
                }
                else
                {
                    if (strName1 == string.Empty)
                    {
                        strName1 = strWords[i];
                    }
                    else
                    {
                        strName2 = strWords[i];
                    }
                }
            }

            customers = new DB.CustomerRepository().Search(strName1, strName2, strPhoneNo, usertoken.CompanyGuid, Guid.Empty);
            if (!customers.Any())
            {
                _mCustomer.Status = RestStatus.NotExisting;
                _mCustomer.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            _mCustomers.Clear();
            foreach(DB.tCustomer customer in customers)
            {
                // Populate
                Models.Customer mCustomer = new Models.Customer();
                mCustomer.CustomerId = customer.AdditionalCustomerNo;
                mCustomer.FirstName = customer.FirstName;
                mCustomer.LastName = customer.LastName;
                mCustomer.Email = customer.Email;
                mCustomer.PhoneNo = customer.PhoneNo;
                mCustomer.Address = customer.Address;
                mCustomer.ZipCode = customer.ZipCode;
                mCustomer.City = customer.City;
                // Cont...
                _mCustomers.Add(mCustomer);
            }

            // Success
            _mCustomer.Status = RestStatus.Success;
            _mCustomer.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        }

        /// <summary>
        /// Add Customer
        /// </summary>
        /// <param name="token"></param>
        /// <param name="customerId">customerId = AdditionalCustomerNo (string)</param>
        /// <returns></returns>
        public HttpResponseMessage Post(string token, string customerId)
        {
            if (!Request.Content.IsFormData())
            {
                _mCustomer.Status = RestStatus.NotFormData;
                _mCustomer.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(customerId))
            {
                _mCustomer.Status = RestStatus.ParameterError;
                _mCustomer.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCustomer.Status = RestStatus.AuthenticationFailed;
                _mCustomer.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strFirstName = string.IsNullOrEmpty(dic["FirstName"]) ? string.Empty : dic["FirstName"];
            string strLastName = string.IsNullOrEmpty(dic["LastName"]) ? string.Empty : dic["LastName"];
            string strPhoneNo = string.IsNullOrEmpty(dic["PhoneNo"]) ? string.Empty : dic["PhoneNo"];
            string strEmail = string.IsNullOrEmpty(dic["Email"]) ? string.Empty : dic["Email"];
            string strAddress = string.IsNullOrEmpty(dic["Address"]) ? string.Empty : dic["Address"];
            string strZipCode = string.IsNullOrEmpty(dic["ZipCode"]) ? string.Empty : dic["ZipCode"];
            string strCity = string.IsNullOrEmpty(dic["City"]) ? string.Empty : dic["City"];

            // Clean
            strPhoneNo = ML.Common.SmsHelper.CleanPhoneNumber(strPhoneNo);

            // Validate
            DB.CustomerRepository customerRepository = new DB.CustomerRepository();
            DB.tCustomer customer = customerRepository.GetByCompanyGuidAndPhoneNo(usertoken.CompanyGuid, strPhoneNo);
            if(customer != null)
            {
                _mCustomer.Status = RestStatus.AlreadyExists;
                _mCustomer.StatusText = "Already Exists";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Lookup customer
            customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(usertoken.CompanyGuid, customerId);
            if (customer == null)
            {
                // Create new customer
                if(new DB.CustomerService().AddUpdateCustomer(
                    usertoken.CompanyGuid
                    , strPhoneNo
                    , Customer.Enums.CustomerType.Api
                    , strFirstName
                    , strLastName
                    , customerId
                    , string.Empty
                    , string.Empty
                    , strEmail
                    , strAddress
                    , strZipCode
                    , strCity
                    ) != DB.Repository.Status.Success)
                {
                    _mCustomer.Status = RestStatus.GenericError;
                    _mCustomer.StatusText = "Generic Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
                }

                // Reget
                customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(usertoken.CompanyGuid, customerId);
            }
            else
            {
                _mCustomer.Status = RestStatus.AlreadyExists;
                _mCustomer.StatusText = "Already Exists";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Populate
            _mCustomer.CustomerId = customer.AdditionalCustomerNo;
            _mCustomer.FirstName = customer.FirstName;
            _mCustomer.LastName = customer.LastName;
            _mCustomer.Email = customer.Email;
            _mCustomer.PhoneNo = customer.PhoneNo;
            _mCustomer.Address = customer.Address;
            _mCustomer.ZipCode = customer.ZipCode;
            _mCustomer.City = customer.City;
            // Cont...

            // Success
            _mCustomer.Status = RestStatus.Success;
            _mCustomer.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        }

        public HttpResponseMessage Put(string token, string customerId)
        {
            if (!Request.Content.IsFormData())
            {
                _mCustomer.Status = RestStatus.NotFormData;
                _mCustomer.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(customerId))
            {
                _mCustomer.Status = RestStatus.ParameterError;
                _mCustomer.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCustomer.Status = RestStatus.AuthenticationFailed;
                _mCustomer.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strFirstName = string.IsNullOrEmpty(dic["FirstName"]) ? string.Empty : dic["FirstName"];
            string strLastName = string.IsNullOrEmpty(dic["LastName"]) ? string.Empty : dic["LastName"];
            string strPhoneNo = string.IsNullOrEmpty(dic["PhoneNo"]) ? string.Empty : dic["PhoneNo"];
            string strEmail = string.IsNullOrEmpty(dic["Email"]) ? string.Empty : dic["Email"];
            string strAddress = string.IsNullOrEmpty(dic["Address"]) ? string.Empty : dic["Address"];
            string strZipCode = string.IsNullOrEmpty(dic["ZipCode"]) ? string.Empty : dic["ZipCode"];
            string strCity = string.IsNullOrEmpty(dic["City"]) ? string.Empty : dic["City"];

            // Clean
            strPhoneNo = ML.Common.SmsHelper.CleanPhoneNumber(strPhoneNo);

            // Lookup customer
            DB.CustomerRepository customerRepository = new DB.CustomerRepository();
            DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(usertoken.CompanyGuid, customerId);
            if (customer != null)
            {
                // Update customer
                if(!string.IsNullOrEmpty(strFirstName))
                {
                    customer.FirstName = strFirstName;
                }
                if (!string.IsNullOrEmpty(strLastName))
                {
                    customer.LastName = strLastName;
                }
                if (!string.IsNullOrEmpty(strPhoneNo))
                {
                    customer.PhoneNo = strPhoneNo;
                }
                if (!string.IsNullOrEmpty(strEmail))
                {
                    customer.Email = strEmail;
                }
                if (!string.IsNullOrEmpty(strAddress))
                {
                    customer.Address = strAddress;
                }
                if (!string.IsNullOrEmpty(strZipCode))
                {
                    customer.ZipCode = strZipCode;
                }
                if (!string.IsNullOrEmpty(strCity))
                {
                    customer.City = strCity;
                }

                if(customerRepository.Save() != DB.Repository.Status.Success)
                {
                    _mCustomer.Status = RestStatus.GenericError;
                    _mCustomer.StatusText = "Generic Error";
                    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
                }
            }
            else
            {
                _mCustomer.Status = RestStatus.NotExisting;
                _mCustomer.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Populate
            _mCustomer.CustomerId = customer.AdditionalCustomerNo;
            _mCustomer.FirstName = customer.FirstName;
            _mCustomer.LastName = customer.LastName;
            _mCustomer.Email = customer.Email;
            _mCustomer.PhoneNo = customer.PhoneNo;
            _mCustomer.Address = customer.Address;
            _mCustomer.ZipCode = customer.ZipCode;
            _mCustomer.City = customer.City;
            // Cont...

            // Success
            _mCustomer.Status = RestStatus.Success;
            _mCustomer.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        }

        public HttpResponseMessage Get(string token, string customerId)
        {
            if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(customerId))
            {
                _mCustomer.Status = RestStatus.ParameterError;
                _mCustomer.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCustomer.Status = RestStatus.AuthenticationFailed;
                _mCustomer.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Lookup customer
            DB.CustomerRepository customerRepository = new DB.CustomerRepository();
            DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(usertoken.CompanyGuid, customerId);
            if (customer == null)
            {
                _mCustomer.Status = RestStatus.NotExisting;
                _mCustomer.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Populate
            _mCustomer.CustomerId = customer.AdditionalCustomerNo;
            _mCustomer.FirstName = customer.FirstName;
            _mCustomer.LastName = customer.LastName;
            _mCustomer.Email = customer.Email;
            _mCustomer.PhoneNo = customer.PhoneNo;
            _mCustomer.Address = customer.Address;
            _mCustomer.ZipCode = customer.ZipCode;
            _mCustomer.City = customer.City;
            // Cont...


            //List<Guid> tagGuids = string.IsNullOrEmpty(dic["Tags"]) ? null : tagGuids = dic["Tags"].Split(',').Select(s => Guid.Parse(s)).ToList();
            //_mCustomer.PunchTicketLogs = new DB.PunchTicketLogService().GetPunchTicketLog(customer.CustomerGuid).ToList<PunchTicketLog>();
            _mCustomer.PunchTicketLogs = null;
            List<string> lst = new DB.PunchTicketLogService().GetPunchTicketLog(customer.CustomerGuid);
            if (lst.Count > 0)
            {
                List<PunchTicketLog> punchTicketLogs = new List<PunchTicketLog>();
                foreach (string str in lst)
                {
                    PunchTicketLog punchTicketLog = new PunchTicketLog();
                    punchTicketLog.Comment = str;
                    punchTicketLogs.Add(punchTicketLog);
                }
                _mCustomer.PunchTicketLogs = punchTicketLogs;
            }

            // Success
            _mCustomer.Status = RestStatus.Success;
            _mCustomer.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        }

        public HttpResponseMessage Delete(string token, string customerId)
        {
            if (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(customerId))
            {
                _mCustomer.Status = RestStatus.ParameterError;
                _mCustomer.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mCustomer.Status = RestStatus.AuthenticationFailed;
                _mCustomer.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Lookup customer
            DB.CustomerRepository customerRepository = new DB.CustomerRepository();
            DB.tCustomer customer = customerRepository.GetByCompanyGuidAndAdditionalCustomerNo(usertoken.CompanyGuid, customerId);
            if (customer == null)
            {
                _mCustomer.Status = RestStatus.NotExisting;
                _mCustomer.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Try delete Customer
            try
            {
                customerRepository.Delete(customer);
                customerRepository.Save();
            }
            catch
            {
                _mCustomer.Status = RestStatus.Illegal;
                _mCustomer.StatusText = "Illegal";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            // Success
            _mCustomer.Status = RestStatus.Success;
            _mCustomer.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        }


    }
}
