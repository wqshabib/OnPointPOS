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
    public class OrderStatController : ApiController
    {
        List<OrderStat> _mOrderStats = new List<OrderStat>();
        OrderStat _mOrderStat = new OrderStat();
        
        public OrderStatController()
        {
            _mOrderStats.Add(_mOrderStat);
        }

        // id = OrderPrinterGuid or ContentCategoryGuid
        public HttpResponseMessage Post(string token, string id)
        {
            if (!Request.Content.IsFormData())
            {
                _mOrderStat.Status = RestStatus.NotFormData;
                _mOrderStat.StatusText = "Not FormData";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderStats));
            }

            if (string.IsNullOrEmpty(token) && !ML.Common.Text.IsGuid(id))
            {
                _mOrderStat.Status = RestStatus.ParameterError;
                _mOrderStat.StatusText = "Parameter Error";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderStats));
            }

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
            {
                _mOrderStat.Status = RestStatus.AuthenticationFailed;
                _mOrderStat.StatusText = "Authentication Failed";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderStats));
            }

            // Prepare
            System.Collections.Specialized.NameValueCollection dic = Request.Content.ReadAsFormDataAsync().Result;

            DateTime dtStartDateTime = string.IsNullOrEmpty(dic["StartDateTime"]) ? Convert.ToDateTime("2000-01-01 00:00:00") : Convert.ToDateTime(dic["StartDateTime"]);
            DateTime dtEndDateTime = string.IsNullOrEmpty(dic["EndDateTime"]) ? DateTime.Now.Date : Convert.ToDateTime(dic["EndDateTime"]);

            // Get Orders
            IQueryable<DB.tOrder> orders = null;
            if (new Guid(id) == Guid.Empty)
            {
                orders = new DB.OrderRepository().GetOrders(usertoken.CompanyGuid, 1000000000, dtStartDateTime, dtEndDateTime, DB.OrderStatusRepository.OrderStatus.Completed);
            }
            else
            {
                IQueryable<DB.tOrderPrinter> orderPrinters = new DB.OrderPrinterRepository().GetByContentCategoryGuid(new Guid(id));
                if (orderPrinters.Count() > 1 || orderPrinters.Count() == 0)
                {
                    orders = new DB.OrderRepository().GetOrders(usertoken.CompanyGuid, 1000000000, dtStartDateTime, dtEndDateTime, DB.OrderStatusRepository.OrderStatus.Completed);
                }
                else
                {
                    orders = new DB.OrderRepository().GetOrders(usertoken.CompanyGuid, orderPrinters.FirstOrDefault().OrderPrinterGuid, 1000000000, dtStartDateTime, dtEndDateTime, DB.OrderStatusRepository.OrderStatus.Completed);
                }
            }

            if(!orders.Any())
            {
                _mOrderStat.Status = RestStatus.NotExisting;
                _mOrderStat.StatusText = "Not Existing";
                return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderStats));
            }

            // Group
            var orderStats =
                (from o in orders
                 group o by System.Data.Entity.DbFunctions.TruncateTime(o.TimeStamp) into g
                 select new { TimeStamp = g.Key.Value, Count = g.Count() }
                ).OrderBy(o => o.TimeStamp);


            // Populate
            _mOrderStats.Clear();
            foreach (var orderStat in orderStats)
            {
                _mOrderStats.Add(new OrderStat(orderStat.TimeStamp.ToShortDateString(), orderStat.Count));
            }

            // Pad empty dates
            //DateTime dtStart = Convert.ToDateTime(_mOrderStats.FirstOrDefault().Date);
            //if(dtStart > DateTime.Now.AddMonths(-1))
            //{
            //    dtStart = DateTime.Now.AddMonths(-1).Date;
            //}

            ////DateTime dtEnd = Convert.ToDateTime(_mOrderStats.LastOrDefault().Date);
            //DateTime dtEnd = DateTime.Now.Date.AddDays(1);
            DateTime dtStart = dtStartDateTime.Date;
            DateTime dtEnd = dtEndDateTime.AddDays(1).Date;
            if (dtEnd > DateTime.Now.Date)
            {
                dtEnd = DateTime.Now.AddDays(1).Date;
            }
            
            for (DateTime day = dtStart.AddDays(1); day < dtEnd; day = day.AddDays(1))
            {
                if(!_mOrderStats.Select(os => os.Date).Contains(day.ToShortDateString().ToString()))
                {
                    _mOrderStats.Add(new OrderStat(day.ToShortDateString(), 0));
                }
            }

            // Sort
            _mOrderStats.OrderBy(os => os.Date);

            // Success
            return ML.Common.RestHelper.ConvertStream((MemoryStream)Serializer.Serialize(_mOrderStats));
        }


    }
}
