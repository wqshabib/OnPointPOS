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
    public class OrderPrinterZipCodeController : ApiController
    {
        private List<dynamic> _mResults = new List<dynamic>();
        private dynamic _mResult = new ExpandoObject();

        public OrderPrinterZipCodeController()
        {
            _mResults.Add(_mResult);
        }

         // id = MenuId
        public HttpResponseMessage Get(string token, string id)
        {

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(id))
            {
                _mResult.Status = RestStatus.ParameterError;
                _mResult.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            /*
            SHAHID:RESTRICTIONS NOT REQUIRED
            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mResult.Status = RestStatus.AuthenticationFailed;
                _mResult.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }
            if (orderPrinter.CompanyGuid != usertoken.CompanyGuid)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }
             */

            Guid guidContentCategoryGuid = new Guid(id);

            DB.tOrderPrinter orderPrinter = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidContentCategoryGuid).FirstOrDefault();
            if (orderPrinter == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }


            // Get ZipCodes
            _mResults.Clear();
            IQueryable<DB.tOrderPrinterZipCode> orderPrinterZipCodes = new DB.OrderPrinterZipCodeRepository().GetOrderPrinterZipCodesByContentCategoryGuid(guidContentCategoryGuid);
            foreach (DB.tOrderPrinterZipCode orderPrinterZipCode in orderPrinterZipCodes)
            {
                _mResult = new ExpandoObject();
                _mResult.ZipCode = orderPrinterZipCode.ZipCode;
                _mResult.DeliveryFee = orderPrinterZipCode.DeliveryFee;
                _mResult.DeliveryMinimumAmount = orderPrinterZipCode.DeliveryMinimumAmount;
                _mResults.Add(_mResult);
            }

            // Success
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }

        // id = MenuId
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

            Guid guidContentCategoryGuid = new Guid(id);

            DB.tOrderPrinter orderPrinter = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidContentCategoryGuid).FirstOrDefault();
            if (orderPrinter == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if (orderPrinter.CompanyGuid != usertoken.CompanyGuid)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strZipCode = string.IsNullOrEmpty(dic["ZipCode"]) ? string.Empty : dic["ZipCode"];
            decimal decDeliveryFee = string.IsNullOrEmpty(dic["DeliveryFee"]) ? 0 : Convert.ToDecimal(dic["DeliveryFee"].Replace('.', ','));
            decimal decDeliveryMinimumAmount = string.IsNullOrEmpty(dic["DeliveryMinimumAmount"]) ? 0 : Convert.ToDecimal(dic["DeliveryMinimumAmount"].Replace('.', ','));

            // Validate
            if (string.IsNullOrEmpty(strZipCode) || !ML.Common.Text.IsNumeric(strZipCode) || strZipCode.Length != 5)
            {
                _mResult.Status = RestStatus.Illegal;
                _mResult.StatusText = "Illegal";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Ensure ZipCode is not existing
            int intZipCode = Convert.ToInt32(strZipCode);
            DB.OrderPrinterZipCodeRepository orderPrinterZipCodeRepository = new DB.OrderPrinterZipCodeRepository();
            DB.tOrderPrinterZipCode orderPrinterZipCode = orderPrinterZipCodeRepository.GetByOrderPrinterGuidAndZipCode(orderPrinter.OrderPrinterGuid, intZipCode);
            if (orderPrinterZipCode != null)
            {
                _mResult.Status = RestStatus.AlreadyExists;
                _mResult.StatusText = "Already Exists";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Ensure ZipCode exists in Postal table
            if(!new DB.PostalRepository().ZipCodeExists(intZipCode))
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Add
            orderPrinterZipCode = new DB.tOrderPrinterZipCode();
            orderPrinterZipCode.ZipCode = intZipCode;
            orderPrinterZipCode.OrderPrinterGuid = orderPrinter.OrderPrinterGuid;
            orderPrinterZipCode.DeliveryFee = decDeliveryFee;
            orderPrinterZipCode.DeliveryMinimumAmount = decDeliveryMinimumAmount;

            DB.Repository.Status status = orderPrinterZipCodeRepository.Save(orderPrinterZipCode);

            if (status != DB.Repository.Status.Success)
            {
                _mResult.Status = RestStatus.GenericError;
                _mResult.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Populate
            _mResult.ZipCode = intZipCode.ToString();
            _mResult.DeliveryFee = decDeliveryFee;
            _mResult.DeliveryMinimumAmount = decDeliveryMinimumAmount;

            // Success
            _mResult.Status = RestStatus.Success;
            _mResult.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }

        // id = MenuId
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

            Guid guidContentCategoryGuid = new Guid(id);

            DB.tOrderPrinter orderPrinter = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidContentCategoryGuid).FirstOrDefault();
            if (orderPrinter == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if (orderPrinter.CompanyGuid != usertoken.CompanyGuid)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;
            string strZipCode = string.IsNullOrEmpty(dic["ZipCode"]) ? string.Empty : dic["ZipCode"];
            decimal decDeliveryFee = string.IsNullOrEmpty(dic["DeliveryFee"]) ? 0 : Convert.ToDecimal(dic["DeliveryFee"].Replace('.', ','));
            decimal decDeliveryMinimumAmount = string.IsNullOrEmpty(dic["DeliveryMinimumAmount"]) ? 0 : Convert.ToDecimal(dic["DeliveryMinimumAmount"].Replace('.', ','));

            // Validate
            if (string.IsNullOrEmpty(strZipCode) || !ML.Common.Text.IsNumeric(strZipCode) || strZipCode.Length != 5)
            {
                _mResult.Status = RestStatus.Illegal;
                _mResult.StatusText = "Illegal";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Ensure ZipCode is existing
            int intZipCode = Convert.ToInt32(strZipCode);
            DB.OrderPrinterZipCodeRepository orderPrinterZipCodeRepository = new DB.OrderPrinterZipCodeRepository();
            DB.tOrderPrinterZipCode orderPrinterZipCode = orderPrinterZipCodeRepository.GetByOrderPrinterGuidAndZipCode(orderPrinter.OrderPrinterGuid, intZipCode);
            if (orderPrinterZipCode == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            //// Ensure ZipCode exists in Postal table
            //if (!new DB.PostalRepository().ZipCodeExists(intZipCode))
            //{
            //    _mResult.Status = RestStatus.NotExisting;
            //    _mResult.StatusText = "Not Existing";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            //}

            // Update
            orderPrinterZipCode.DeliveryFee = decDeliveryFee;
            orderPrinterZipCode.DeliveryMinimumAmount = decDeliveryMinimumAmount;
            
            DB.Repository.Status status = orderPrinterZipCodeRepository.Save();

            if (status != DB.Repository.Status.Success)
            {
                _mResult.Status = RestStatus.GenericError;
                _mResult.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Populate
            _mResult.ZipCode = intZipCode.ToString();
            _mResult.DeliveryFee = decDeliveryFee;
            _mResult.DeliveryMinimumAmount = decDeliveryMinimumAmount;

            // Success
            _mResult.Status = RestStatus.Success;
            _mResult.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }

        // id = MenuId
        public HttpResponseMessage Delete(string token, string id, string zipCode)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(zipCode))
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

            Guid guidContentCategoryGuid = new Guid(id);

            DB.tOrderPrinter orderPrinter = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidContentCategoryGuid).FirstOrDefault();
            if (orderPrinter == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            if (orderPrinter.CompanyGuid != usertoken.CompanyGuid)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Validate
            if (string.IsNullOrEmpty(zipCode) || !ML.Common.Text.IsNumeric(zipCode) || zipCode.Length != 5)
            {
                _mResult.Status = RestStatus.Illegal;
                _mResult.StatusText = "Illegal";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Ensure ZipCode is not existing
            int intZipCode = Convert.ToInt32(zipCode);
            DB.OrderPrinterZipCodeRepository orderPrinterZipCodeRepository = new DB.OrderPrinterZipCodeRepository();
            DB.tOrderPrinterZipCode orderPrinterZipCode = orderPrinterZipCodeRepository.GetByOrderPrinterGuidAndZipCode(orderPrinter.OrderPrinterGuid, intZipCode);
            if (orderPrinterZipCode == null)
            {
                _mResult.Status = RestStatus.NotExisting;
                _mResult.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            }

            // Delete
            orderPrinterZipCodeRepository.Delete(orderPrinterZipCode);
            DB.Repository.Status status = orderPrinterZipCodeRepository.Save();

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
