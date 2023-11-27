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
    public class CustomerSearchController : ApiController
    {
        List<Models.Customer> _mCustomers = new List<Models.Customer>();
        Models.Customer _mCustomer = new Models.Customer();
        
        public CustomerSearchController()
        {
            _mCustomers.Add(_mCustomer);
        }

        public HttpResponseMessage Post(string token, int take)
        {
            if (!Request.Content.IsFormData())
            {
                _mCustomer.Status = RestStatus.NotFormData;
                _mCustomer.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
            }

            if (string.IsNullOrEmpty(token) && take < 1)
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

            //DateTime dtStartDateTime = string.IsNullOrEmpty(dic["StartDateTime"]) ? Convert.ToDateTime("2000-01-01 00:00:00") : Convert.ToDateTime(dic["StartDateTime"]);
            //DateTime dtEndDateTime = string.IsNullOrEmpty(dic["EndDateTime"]) ? DateTime.Now : Convert.ToDateTime(dic["EndDateTime"]);
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

            customers = new DB.CustomerRepository().Search(strName1, strName2, strPhoneNo, usertoken.CompanyGuid, Guid.Empty).Take(take);

            // Populate
            _mCustomers.Clear();
            foreach (DB.tCustomer customer in customers)
            {
                _mCustomer = new Models.Customer();
                _mCustomer.CustomerId = customer.CustomerGuid.ToString();


                if (string.IsNullOrEmpty(customer.FirstName) && string.IsNullOrEmpty(customer.LastName) && !string.IsNullOrEmpty(customer.PhoneNo))
                {
                    _mCustomer.FirstName = customer.PhoneNo;
                    _mCustomer.LastName = string.Empty;
                    _mCustomer.PhoneNo = string.Empty;
                }
                else
                {
                    _mCustomer.FirstName = customer.FirstName;
                    _mCustomer.LastName = customer.LastName;
                    _mCustomer.PhoneNo = customer.PhoneNo;
                }
                // Cont...

                _mCustomers.Add(_mCustomer);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mCustomers));
        }




    }
}
