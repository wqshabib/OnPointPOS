using ML.Common.Handlers.Serializers;
using ML.Rest2.Helper;
using ML.Rest2.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Xml;

namespace ML.Rest2.Controllers
{
    public class OrderPrinterAvailabilityController : ApiController
    {
        //private List<OrderPrinter> _mOrderPrinters = new List<OrderPrinter>();
        //private OrderPrinter _mOrderPrinter = new OrderPrinter();

        //private List<OrderPrinterAvailability> _mOrderPrinterAvailabilities = new List<OrderPrinterAvailability>();
        //private OrderPrinterAvailability _mOrderPrinterAvailability = new OrderPrinterAvailability();

        private List<dynamic> _mOrderPrinters = new List<dynamic>();
        private dynamic _mOrderPrinter = new ExpandoObject();

        private List<dynamic> _mOrderPrinterAvailabilities = new List<dynamic>();
        private dynamic _mOrderPrinterAvailability = new ExpandoObject();

        public OrderPrinterAvailabilityController()
        {
            _mOrderPrinters.Add(_mOrderPrinter);
            _mOrderPrinterAvailabilities.Add(_mOrderPrinterAvailability);
        }


        public HttpResponseMessage Get(string token, string id)
        {
            return Get(token, id, (int)DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Regular);
        }

        /// <summary>
        /// id = ContentCategoryGuid (Connected to an OrderPrinter)
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string token, string id, int orderPrinterAvailabilityType)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id) && orderPrinterAvailabilityType < 1)
            {
                _mOrderPrinter.Status = RestStatus.ParameterError;
                _mOrderPrinter.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinters));
            }

            DB.UserToken userToken = DB.UserTokenService.ValidateUserToken(token);
            if (!userToken.Valid)
            {
                _mOrderPrinter.Status = RestStatus.AuthenticationFailed;
                _mOrderPrinter.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinters));
            }

            Guid guidContentCategoryGuid = new Guid(id);
            IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(guidContentCategoryGuid);
            if (!orderPrinters.Any())
            {
                _mOrderPrinter.Status = RestStatus.NotExisting;
                _mOrderPrinter.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinters));
            }
            if (orderPrinters.FirstOrDefault().CompanyGuid != userToken.CompanyGuid)
            {
                _mOrderPrinter.Status = RestStatus.NotExisting;
                _mOrderPrinter.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinters));
            }

            DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType theOrderPrinterAvailabilityType = (DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType)Enum.Parse(typeof(DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType), orderPrinterAvailabilityType.ToString());

            // Populate
            _mOrderPrinters.Clear();
            foreach(DB.tOrderPrinter orderPrinter in orderPrinters)
            {
                _mOrderPrinter = new ExpandoObject();
                _mOrderPrinter.OrderPrinterId = orderPrinter.OrderPrinterGuid.ToString();

                List<dynamic> mOrderPrinterAvailabilities = new List<dynamic>();
                dynamic mOrderPrinterAvailability = new ExpandoObject();
                foreach (DB.tOrderPrinterAvailability orderPrinterAvailability in orderPrinter.tOrderPrinterAvailability.Where(opa => opa.OrderPrinterAvailabilityTypeID == (int)theOrderPrinterAvailabilityType))
                {
                    mOrderPrinterAvailability = new ExpandoObject();
                    mOrderPrinterAvailability.OrderPrinterAvailabilityId = orderPrinterAvailability.OrderPrinterAvailabilityGuid.ToString();
                    mOrderPrinterAvailability.Date = orderPrinterAvailability.Date;
                    mOrderPrinterAvailability.WeekDay = orderPrinterAvailability.WeekDay;
                    mOrderPrinterAvailability.StartTime = orderPrinterAvailability.StartTime;
                    mOrderPrinterAvailability.EndTime = orderPrinterAvailability.EndTime;
                    mOrderPrinterAvailability.OrderPrinterAvailabilityType = (int)orderPrinterAvailability.OrderPrinterAvailabilityTypeID;
                    
                    if(!string.IsNullOrEmpty(orderPrinterAvailability.DeliveryStartTime))
                    {
                        mOrderPrinterAvailability.DeliveryStartTime = orderPrinterAvailability.DeliveryStartTime;
                    }
                    if(!string.IsNullOrEmpty(orderPrinterAvailability.DeliveryEndTime))
                    {
                        mOrderPrinterAvailability.DeliveryEndTime = orderPrinterAvailability.DeliveryEndTime;
                    }

                    mOrderPrinterAvailabilities.Add(mOrderPrinterAvailability);
                }
                _mOrderPrinter.OrderPrinterAvailability = mOrderPrinterAvailabilities;
                _mOrderPrinter.Status = 0;
                _mOrderPrinter.StatusText = "Success";

                _mOrderPrinters.Add(_mOrderPrinter);
            }

            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinters));
        }





        public HttpResponseMessage Post(string token, string id)
        {
            return Post(token, id, (int)DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType.Regular);
        }

        /// <summary>
        /// id = OrderPrinterGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Post(string token, string id, int orderPrinterAvailabilityType)
        {
            if (!Request.Content.IsFormData())
            {
                _mOrderPrinterAvailability.Status = RestStatus.NotFormData;
                _mOrderPrinterAvailability.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mOrderPrinterAvailability.Status = RestStatus.ParameterError;
                _mOrderPrinterAvailability.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mOrderPrinterAvailability.Status = RestStatus.AuthenticationFailed;
                _mOrderPrinterAvailability.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            Guid guidOrderPrinterGuid = new Guid(id);
            DB.tOrderPrinter orderPrinter = new DB.OrderPrinterRepository().GetByOrderPrinterGuid(guidOrderPrinterGuid);
            if (orderPrinter == null)
            {
                _mOrderPrinterAvailability.Status = RestStatus.NotExisting;
                _mOrderPrinterAvailability.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }
            if (orderPrinter.CompanyGuid != usertoken.CompanyGuid)
            {
                _mOrderPrinterAvailability.Status = RestStatus.NotExisting;
                _mOrderPrinterAvailability.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }



            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            string strDate = dic["Date"] == null ? string.Empty : dic["Date"];
            string strWeekDay = dic["WeekDay"] == null ? string.Empty : dic["WeekDay"];
            string strStartTime = dic["StartTime"] == null ? string.Empty : dic["StartTime"];
            string strEndTime = dic["EndTime"] == null ? string.Empty : dic["EndTime"];

            string strDeliveryStartTime = dic["DeliveryStartTime"] == null ? string.Empty : dic["DeliveryStartTime"];
            string strDeliveryEndTime = dic["DeliveryEndTime"] == null ? string.Empty : dic["DeliveryEndTime"];

            DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType theOrderPrinterAvailabilityType = (DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType)Enum.Parse(typeof(DB.OrderPrinterAvailabilityRepository.OrderPrinterAvailabilityType), orderPrinterAvailabilityType.ToString());

            // Validate
            DB.tOrderPrinterAvailability orderPrinterAvailability = new DB.OrderPrinterAvailabilityRepository().GetOrderPrinterAvailability(guidOrderPrinterGuid, strDate, strWeekDay, strStartTime, strEndTime, theOrderPrinterAvailabilityType);
            if (orderPrinterAvailability != null)
            {
                _mOrderPrinterAvailability.Status = RestStatus.AlreadyExists;
                _mOrderPrinterAvailability.StatusText = "Already Exists";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            // Save
            orderPrinterAvailability = new DB.tOrderPrinterAvailability();
            orderPrinterAvailability.OrderPrinterAvailabilityGuid = Guid.NewGuid();
            orderPrinterAvailability.OrderPrinterGuid = guidOrderPrinterGuid;
            orderPrinterAvailability.Date = strDate;
            orderPrinterAvailability.WeekDay = strWeekDay;
            orderPrinterAvailability.StartTime = strStartTime;
            orderPrinterAvailability.EndTime = strEndTime;
            orderPrinterAvailability.DeliveryStartTime = strDeliveryStartTime;
            orderPrinterAvailability.DeliveryEndTime = strDeliveryEndTime;
            orderPrinterAvailability.OrderPrinterAvailabilityTypeID = (int)theOrderPrinterAvailabilityType;

            if (new DB.OrderPrinterAvailabilityRepository().Save(orderPrinterAvailability) != DB.Repository.Status.Success)
            {
                _mOrderPrinterAvailability.Status = RestStatus.GenericError;
                _mOrderPrinterAvailability.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            // Populate result
            _mOrderPrinterAvailability.OrderPrinterAvailabilityId = orderPrinterAvailability.OrderPrinterAvailabilityGuid.ToString();
            //_mOrderPrinterAvailability.OrderPrinterId = orderPrinterAvailability.OrderPrinterGuid.ToString();
            _mOrderPrinterAvailability.Date = orderPrinterAvailability.Date;
            _mOrderPrinterAvailability.WeekDay = orderPrinterAvailability.WeekDay;
            _mOrderPrinterAvailability.StartTime = orderPrinterAvailability.StartTime;
            _mOrderPrinterAvailability.EndTime = orderPrinterAvailability.EndTime;
            _mOrderPrinterAvailability.OrderPrinterAvailabilityType = (int)theOrderPrinterAvailabilityType;
            _mOrderPrinterAvailability.DeliveryStartTime = orderPrinterAvailability.DeliveryStartTime;
            _mOrderPrinterAvailability.DeliveryEndTime = orderPrinterAvailability.DeliveryEndTime;

            _mOrderPrinterAvailability.Status = RestStatus.Success;
            _mOrderPrinterAvailability.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
        }


        /// <summary>
        /// id = OrderPrinterAvailabilityGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Put(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mOrderPrinterAvailability.Status = RestStatus.NotFormData;
                _mOrderPrinterAvailability.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mOrderPrinterAvailability.Status = RestStatus.ParameterError;
                _mOrderPrinterAvailability.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mOrderPrinterAvailability.Status = RestStatus.AuthenticationFailed;
                _mOrderPrinterAvailability.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }


            Guid guidOrderPrinterAvailabilityGuid = new Guid(id);
            DB.OrderPrinterAvailabilityRepository orderPrinterAvailabilityRepository = new DB.OrderPrinterAvailabilityRepository();
            DB.tOrderPrinterAvailability orderPrinterAvailability = orderPrinterAvailabilityRepository.GetByOrderPrinterAvailabilityGuid(guidOrderPrinterAvailabilityGuid);
            if (orderPrinterAvailability == null)
            {
                _mOrderPrinterAvailability.Status = RestStatus.NotExisting;
                _mOrderPrinterAvailability.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }
            if (orderPrinterAvailability.tOrderPrinter.CompanyGuid != usertoken.CompanyGuid)
            {
                _mOrderPrinterAvailability.Status = RestStatus.NotExisting;
                _mOrderPrinterAvailability.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            string strDate = dic["Date"] == null ? string.Empty : dic["Date"];
            string strWeekDay = dic["WeekDay"] == null ? string.Empty : dic["WeekDay"];
            string strStartTime = dic["StartTime"] == null ? string.Empty : dic["StartTime"];
            string strEndTime = dic["EndTime"] == null ? string.Empty : dic["EndTime"];

            string strDeliveryStartTime = dic["DeliveryStartTime"] == null ? string.Empty : dic["DeliveryStartTime"];
            string strDeliveryEndTime = dic["DeliveryEndTime"] == null ? string.Empty : dic["DeliveryEndTime"];

            // Save
            orderPrinterAvailability.Date = strDate;
            orderPrinterAvailability.WeekDay = strWeekDay;
            orderPrinterAvailability.StartTime = strStartTime;
            orderPrinterAvailability.EndTime = strEndTime;
            orderPrinterAvailability.DeliveryStartTime = strDeliveryStartTime;
            orderPrinterAvailability.DeliveryEndTime = strDeliveryEndTime;

            if (orderPrinterAvailabilityRepository.Save() != DB.Repository.Status.Success)
            {
                _mOrderPrinterAvailability.Status = RestStatus.GenericError;
                _mOrderPrinterAvailability.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            // Populate result
            _mOrderPrinterAvailability.OrderPrinterAvailabilityId = orderPrinterAvailability.OrderPrinterAvailabilityGuid.ToString();
            //_mOrderPrinterAvailability.OrderPrinterId = orderPrinterAvailability.OrderPrinterGuid.ToString();
            _mOrderPrinterAvailability.Date = orderPrinterAvailability.Date;
            _mOrderPrinterAvailability.WeekDay = orderPrinterAvailability.WeekDay;
            _mOrderPrinterAvailability.StartTime = orderPrinterAvailability.StartTime;
            _mOrderPrinterAvailability.EndTime = orderPrinterAvailability.EndTime;
            _mOrderPrinterAvailability.OrderPrinterAvailabilityType = (int)orderPrinterAvailability.OrderPrinterAvailabilityTypeID;
            _mOrderPrinterAvailability.DeliveryStartTime = orderPrinterAvailability.DeliveryStartTime;
            _mOrderPrinterAvailability.DeliveryEndTime = orderPrinterAvailability.DeliveryEndTime;

            _mOrderPrinterAvailability.Status = RestStatus.Success;
            _mOrderPrinterAvailability.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
        }

        /// <summary>
        /// id = OrderPrinterAvailabilityGuid
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage Delete(string token, string id)
        {
            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuidNotEmpty(id))
            {
                _mOrderPrinterAvailability.Status = RestStatus.ParameterError;
                _mOrderPrinterAvailability.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mOrderPrinterAvailability.Status = RestStatus.AuthenticationFailed;
                _mOrderPrinterAvailability.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            Guid OrderPrinterAvailabilityGuid = new Guid(id);
            DB.OrderPrinterAvailabilityRepository orderPrinterAvailabilityRepository = new DB.OrderPrinterAvailabilityRepository();
            DB.tOrderPrinterAvailability orderPrinterAvailability = orderPrinterAvailabilityRepository.GetByOrderPrinterAvailabilityGuid(OrderPrinterAvailabilityGuid);
            if (orderPrinterAvailability == null)
            {
                _mOrderPrinterAvailability.Status = RestStatus.NotExisting;
                _mOrderPrinterAvailability.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }
            if (orderPrinterAvailability.tOrderPrinter.CompanyGuid != usertoken.CompanyGuid)
            {
                _mOrderPrinterAvailability.Status = RestStatus.NotExisting;
                _mOrderPrinterAvailability.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            // Delete
            orderPrinterAvailabilityRepository.Delete(orderPrinterAvailability);
            if (orderPrinterAvailabilityRepository.Save() != DB.Repository.Status.Success)
            {
                _mOrderPrinterAvailability.Status = RestStatus.GenericError;
                _mOrderPrinterAvailability.StatusText = "Generic Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
            }

            // Populate result
            _mOrderPrinterAvailability.Status = RestStatus.Success;
            _mOrderPrinterAvailability.StatusText = "Success";
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderPrinterAvailabilities));
        }


    }
}
