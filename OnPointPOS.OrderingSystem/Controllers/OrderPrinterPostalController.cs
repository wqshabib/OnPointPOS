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
    public class OrderPrinterPostalController : ApiController
    {
        private List<dynamic> _mResults = new List<dynamic>();
        private dynamic _mResult = new ExpandoObject();

        public OrderPrinterPostalController()
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

            //11/25/2015: Zahid : class not found [OrderPrinterPostalRepository] in DB Project
            // Get Postals
            _mResults.Clear();
            //IQueryable<DB.tOrderPrinterPostal> orderPrinterPostals = new DB.OrderPrinterPostalRepository().GetOrderPrinterPostalsByContentCategoryGuid(guidContentCategoryGuid);
            //foreach (var orderPrinterPostal in orderPrinterPostals)
            //{
            //    _mResult = new ExpandoObject();
            //    _mResult.PostalId = orderPrinterPostal.PostalGuid.ToString();
            //    _mResult.Address = new DB.PostalService().GetAddressLine(orderPrinterPostal.tPostal);
            //    _mResult.DeliveryFee = orderPrinterPostal.DeliveryFee;
            //    _mResult.DeliveryMinimumAmount = orderPrinterPostal.DeliveryMinimumAmount;
            //    _mResults.Add(_mResult);
            //}

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
            Guid guidPostalGuid = new Guid(dic["PostalId"]);
            decimal decDeliveryFee = string.IsNullOrEmpty(dic["DeliveryFee"]) ? 0 : Convert.ToDecimal(dic["DeliveryFee"].Replace('.', ','));
            decimal decDeliveryMinimumAmount = string.IsNullOrEmpty(dic["DeliveryMinimumAmount"]) ? 0 : Convert.ToDecimal(dic["DeliveryMinimumAmount"].Replace('.', ','));

            //11/25/2015: Zahid : class not found [OrderPrinterPostalRepository] in DB Project
            // Ensure Postal is not existing
            //DB.OrderPrinterPostalRepository orderPrinterPostalRepository = new DB.OrderPrinterPostalRepository();
            //DB.tOrderPrinterPostal orderPrinterPostal = orderPrinterPostalRepository.GetByOrderPrinterGuidAndPostalGuid(orderPrinter.OrderPrinterGuid, guidPostalGuid);
            //if (orderPrinterPostal != null)
            //{
            //    _mResult.Status = RestStatus.AlreadyExists;
            //    _mResult.StatusText = "Already Exists";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            //}

            //// Ensure Postal exists in Postal table
            //if (new DB.PostalRepository().GetPostal(guidPostalGuid) == null)
            //{
            //    _mResult.Status = RestStatus.NotExisting;
            //    _mResult.StatusText = "Not Existing";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            //}

            //// Add
            //orderPrinterPostal = new DB.tOrderPrinterPostal();
            //orderPrinterPostal.PostalGuid = guidPostalGuid;
            //orderPrinterPostal.OrderPrinterGuid = orderPrinter.OrderPrinterGuid;
            //orderPrinterPostal.DeliveryFee = decDeliveryFee;
            //orderPrinterPostal.DeliveryMinimumAmount = decDeliveryMinimumAmount;

            //DB.Repository.Status status = orderPrinterPostalRepository.Save(orderPrinterPostal);

            //if (status != DB.Repository.Status.Success)
            //{
            //    _mResult.Status = RestStatus.GenericError;
            //    _mResult.StatusText = "Generic Error";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            //}

            //// Populate
            //_mResult.PostalId = orderPrinterPostal.PostalGuid.ToString();
            //_mResult.Address = new DB.PostalService().GetAddressLine(orderPrinterPostal.tPostal);
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
            Guid guidPostalGuid = new Guid(dic["PostalId"]);
            decimal decDeliveryFee = string.IsNullOrEmpty(dic["DeliveryFee"]) ? 0 : Convert.ToDecimal(dic["DeliveryFee"].Replace('.', ','));
            decimal decDeliveryMinimumAmount = string.IsNullOrEmpty(dic["DeliveryMinimumAmount"]) ? 0 : Convert.ToDecimal(dic["DeliveryMinimumAmount"].Replace('.', ','));

            //11/25/2015: Zahid : class not found [OrderPrinterPostalRepository] in DB Project

            //// Ensure Postal is existing
            //DB.OrderPrinterPostalRepository orderPrinterPostalRepository = new DB.OrderPrinterPostalRepository();
            //DB.tOrderPrinterPostal orderPrinterPostal = orderPrinterPostalRepository.GetByOrderPrinterGuidAndPostalGuid(orderPrinter.OrderPrinterGuid, guidPostalGuid);
            //if (orderPrinterPostal == null)
            //{
            //    _mResult.Status = RestStatus.NotExisting;
            //    _mResult.StatusText = "Not Existing";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            //}

            //// Update
            //orderPrinterPostal.DeliveryFee = decDeliveryFee;
            //orderPrinterPostal.DeliveryMinimumAmount = decDeliveryMinimumAmount;

            //DB.Repository.Status status = orderPrinterPostalRepository.Save();

            //if (status != DB.Repository.Status.Success)
            //{
            //    _mResult.Status = RestStatus.GenericError;
            //    _mResult.StatusText = "Generic Error";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            //}

            //// Populate
            //_mResult.PostalId = orderPrinterPostal.PostalGuid.ToString();
            //_mResult.Address = new DB.PostalService().GetAddressLine(orderPrinterPostal.tPostal);


            _mResult.DeliveryFee = decDeliveryFee;
            _mResult.DeliveryMinimumAmount = decDeliveryMinimumAmount;

            // Success
            _mResult.Status = RestStatus.Success;
            _mResult.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }

        // id = MenuId
        public HttpResponseMessage Delete(string token, string id, string postalId)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(id) || !ML.Common.Text.IsGuidNotEmpty(postalId))
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

            //11/25/2015: Zahid : class not found [OrderPrinterPostalRepository] in DB Project
            // Ensure Postal is not existing
            Guid guidPostalGuid = new Guid(postalId);
            //DB.OrderPrinterPostalRepository orderPrinterPostalRepository = new DB.OrderPrinterPostalRepository();
            //DB.tOrderPrinterPostal orderPrinterPostal = orderPrinterPostalRepository.GetByOrderPrinterGuidAndPostalGuid(orderPrinter.OrderPrinterGuid, guidPostalGuid);
            //if (orderPrinterPostal == null)
            //{
            //    _mResult.Status = RestStatus.NotExisting;
            //    _mResult.StatusText = "Not Existing";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            //}

            //// Delete
            //orderPrinterPostalRepository.Delete(orderPrinterPostal);
            //DB.Repository.Status status = orderPrinterPostalRepository.Save();

            //if (status != DB.Repository.Status.Success)
            //{
            //    _mResult.Status = RestStatus.Illegal;
            //    _mResult.StatusText = "Illegal";
            //    return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
            //}

            // Success
            _mResult.Status = RestStatus.Success;
            _mResult.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mResults));
        }




    }
}
