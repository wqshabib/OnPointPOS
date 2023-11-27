using ML.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using ML.Rest2;
using ML.Rest2.Models;
using System.IO;
using ML.Common.Handlers.Serializers;
using System.Dynamic;
using System.Net.Http;
using System.Globalization;

namespace ML.WebAPI.Controllers
{

    //[RequireHttps]
    // [EnableCors(origins: "*", headers: "*", methods: "*")]
    //[Authorize]
    [RoutePrefix("api/Drivify")]
    public class DrivifyController : ApiController
    {
        private List<PendingOrder> _mPendingOrderLists = new List<PendingOrder>();
        public DrivifyController()
        {
        }

        /// <summary>
        /// url:api/Driver/DeliveryDenied, Purpose:deny the order for current driver, order guid is mandatory.
        /// </summary>
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("DeliveryDenied")]
        public IHttpActionResult DeliveryDenied(DeliveryDeniedValue data)
        {
            if (string.IsNullOrEmpty(data.token))
                return Ok("Parameter Error");

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(data.token);
            if (!usertoken.Valid)
                return Ok("Authentication Failed");
            Guid orderId;
            Guid.TryParse(data.OrderGuid, out orderId);
            if (orderId == Guid.Empty)
                return Ok("Invalid Order Id");

            //DEASSOCIATE CURRENT USER FROM ORDER
            //ASSOCIATE NEW USER TO ORDER
            return Ok("Post order status sucessfully.");
        }

        /// <summary>
        /// url:api/Driver/UpdateOrderStatus, Purpose: update the status,PickUpTime and DeliveryDateTime against any order, order guid is mandatory.
        /// </summary>
        /// <param name="OrderGuid"></param>
        /// <param name="orderStatus"></param>   
        /// <param name="PickUpTime"></param> 
        /// <param name="DeliveryDateTime"></param>     
        /// <returns></returns>

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateOrderStatus")]
        public IHttpActionResult UpdateOrderStatus(OrderStatusValue orderStatus)
        {
            if (string.IsNullOrEmpty(orderStatus.token))
                return Ok("Parameter Error");

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(orderStatus.token);
            if (!usertoken.Valid)
                return Ok("Authentication Failed");
            Guid orderId;
            Guid.TryParse(orderStatus.OrderGuid, out orderId);
            if (orderId == Guid.Empty)
                return Ok("Invalid Order Id");

            var deliveryTime = CleanApiString(orderStatus.DeliveryTime);
            var pickUpTime = CleanApiString(orderStatus.PickUpTime);

            if ((int) DB.OrderStatusRepository.OrderStatus.Completed == orderStatus.OrderStatus || (int) DB.OrderStatusRepository.OrderStatus.DriverConfirmed == orderStatus.OrderStatus)
            {   
                if (!string.IsNullOrEmpty(deliveryTime) || !string.IsNullOrEmpty(pickUpTime))
                {
                    var OrderRepository = new DB.OrderRepository();
                    DB.tOrder orderObj = OrderRepository.GetByOrderGuid(orderId);
                    if (orderObj != null)
                    {
                        DateTime dt;
                        DateTime.TryParse(deliveryTime, out dt);
                        if (dt != DateTime.MinValue)
                            orderObj.DeliveryDateTime = dt;
                        DateTime.TryParse(pickUpTime, out dt);
                        if (dt != DateTime.MinValue)
                            orderObj.PickupDateTime = dt;
                        OrderRepository.Save();
                    }

                }
            }
            DB.tOrder order = new DB.OrderRepository().GetByOrderGuid(orderId);
            if (order == null)
                return Ok("Order Not Exist.");
            if (DB.OrderStatusService.HasOrderStatus(orderId, (DB.OrderStatusRepository.OrderStatus)orderStatus.OrderStatus))
                return Ok("Order status already exist.");

            if ((int)DB.OrderStatusRepository.OrderStatus.Completed == orderStatus.OrderStatus)
            {
                //BUSINESS RULE
                DB.OrderStatusService.AddOrderStatus(order, OrderStatusRepository.OrderStatus.DriverAcknowledged, DB.OrderStatusRepository.ClientType.Printer);
            }
            if ((int)DB.OrderStatusRepository.OrderStatus.Delivered == orderStatus.OrderStatus || (int)DB.OrderStatusRepository.OrderStatus.Completed == orderStatus.OrderStatus || (int)DB.OrderStatusRepository.OrderStatus.DriverConfirmed == orderStatus.OrderStatus || (int)DB.OrderStatusRepository.OrderStatus.ReadyToPickup == orderStatus.OrderStatus || (int)DB.OrderStatusRepository.OrderStatus.OnWay == orderStatus.OrderStatus)
            {
                if (!DB.OrderStatusService.HasOrderStatus(orderId, DB.OrderStatusRepository.OrderStatus.Processing))
                {
                    DB.OrderStatusService.AddOrderStatus(order, DB.OrderStatusRepository.OrderStatus.Processing, DB.OrderStatusRepository.ClientType.Printer);
                }
                DB.OrderStatusService.AddOrderStatus(order, (DB.OrderStatusRepository.OrderStatus)orderStatus.OrderStatus, DB.OrderReasonCode.NoReason, pickUpTime, orderStatus.DeliveryTime, DB.OrderStatusRepository.ClientType.Printer);

            }
            else
            {
                if (!string.IsNullOrEmpty(orderStatus.DeliveryTime))
                {
                    orderStatus.DeliveryTime = order.DeliveryDateTime.ToShortTimeString();
                }
                DB.OrderStatusService.AddOrderStatus(order, (DB.OrderStatusRepository.OrderStatus)orderStatus.OrderStatus, DB.OrderReasonCode.NoReason, pickUpTime, orderStatus.DeliveryTime, DB.OrderStatusRepository.ClientType.Printer);
            }
            return Ok("Post order status sucessfully.");
        }




        /// <summary>
        /// url:'api/Drivify/GetPendingOrders, Purpose=get the pending order.
        /// </summary>
        /// <returns>List of Pending order</returns>
        /// 

        [HttpGet]
        [Route("GetPendingOrders")]
        public IHttpActionResult GetPendingOrders(string token)
        {
            if (string.IsNullOrEmpty(token))
                return Ok("Parameter Error");

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
                return Ok("Authentication Failed");

            var tUser = new DB.UserRepository().GetByUserGuid(usertoken.UserGuid);

            var orders = new DB.OrderRepository().GetDriverPendingOrders(tUser.CompanyGuid, tUser.tUserPrinterBridge.ToList()).ToList();

            _mPendingOrderLists.Clear();

            foreach (var order in orders)
            {
                var _mPendingOrder = new PendingOrder(order);
                _mPendingOrder.SetDrivifyColorCode();
                _mPendingOrder.PickupDateTime = order.PickupDateTime.HasValue ? order.PickupDateTime.Value.ToString("yyyy-MM-dd HH:mm") : order.DeliveryDateTime.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm");
                _mPendingOrderLists.Add(_mPendingOrder);
            }
            return Ok(_mPendingOrderLists);
        }
        /// <summary>
        ///  url:'api/Drivify/DeliveryDenied, Purpose=to denie the order delivery by driver.
        /// </summary>
        /// <param name="deniedModel">deniedModel(token,orderId,driverId,companyID,reason)</param>
        /// <returns></returns>
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("DeliveryDenied")]
        public IHttpActionResult DeliveryDenied(OrderDeliveryDeniedModel deniedModel)
        {
            try
            {
                if (string.IsNullOrEmpty(deniedModel.token))
                    return Ok("Parameter Error");

                DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(deniedModel.token);
                if (!usertoken.Valid)
                    return Ok("Authentication Failed");
                Guid orderGuid = Guid.Parse(deniedModel.orderId);
                Guid driverGuid = Guid.Parse(deniedModel.driverId);
                Guid companyGuid = Guid.Parse(deniedModel.companyId);
                Repository.Status status = new DB.OrderRepository().OrderDeliveryDenied(companyGuid, orderGuid, driverGuid, deniedModel.reason);
                if (status == Repository.Status.Success)
                    return Ok("Order Denied sucessfully.");
                else
                    return Ok("Error duing drder denied.");
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }
        /// <summary>
        /// url:'api/ShopPrinter/GetOrderHistoryCompleted, Purpose=get all current date completed order against each driver.
        /// </summary>
        /// <returns>List of completed Order</returns>
        /// 

        [HttpGet]
        [Route("GetOrderHistoryCompleted")]
        public IHttpActionResult GetOrderHistoryCompleted(string token)
        {
            if (string.IsNullOrEmpty(token))
                return Ok("Parameter Error");

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
                return Ok("Authentication Failed");

            var tUser = new DB.UserRepository().GetByUserGuid(usertoken.UserGuid);

            var orders = new DB.OrderRepository().GetDriversOrderHistoryCompleted(tUser.CompanyGuid, tUser.tUserPrinterBridge.ToList()).ToList();

            _mPendingOrderLists.Clear();

            foreach (var order in orders)
            {
                var _mPendingOrder = new PendingOrder(order);
                _mPendingOrder.SetDrivifyColorCode();
                _mPendingOrderLists.Add(_mPendingOrder);
            }
            return Ok(_mPendingOrderLists);

        }


        /// <summary>
        /// url:'api/Drivify/GetOrderDetail, Purpose=get all item detail against current order.
        /// </summary>
        /// <returns> Order detail</returns>
        /// 

        [HttpGet]
        [Route("GetOrderDetail")]
        public IHttpActionResult GetOrderDetail(string token, string OrderGuid)
        {
            if (string.IsNullOrEmpty(token))
                return Ok("Parameter Error");

            DB.UserToken usertoken = DB.UserTokenService.ValidateUserToken(token);
            if (!usertoken.Valid)
                return Ok("Authentication Failed");

            var order = new DB.OrderRepository().GetByOrderGuid(Guid.Parse(OrderGuid));
            var _mPendingOrder = new PendingOrder(order);
            _mPendingOrder.SetDrivifyColorCode();
            //NEED TO SET DINAMIC VALUES UNDER DRIVIFY
            //_mPendingOrder.IsDeniable = true/false;
            _mPendingOrder.OrderItem = new List<OrderItem>();

            var obj = new OrderService();
            var Receipt = obj.PrepareReceipt(order);

            foreach (var receipt in Receipt)
            {

                var orderItem = new OrderItem
                {
                    Title = receipt.Title,
                    Price = receipt.Price,
                    Quantity = receipt.Quantity,
                    VAT = receipt.VAT,
                    ContentVariant = receipt.ContentVariant
                };

                _mPendingOrder.OrderItem.Add(orderItem);

                orderItem.ReceiptItems = new List<ReceiptItems>();

                foreach (DB.ReceiptItem receiptItem in receipt)
                {
                    var receiptItemObj = new ReceiptItems
                    {
                        ContentGuid = receiptItem.ContentGuid,
                        Extra = receiptItem.Extra,
                        Strip = receiptItem.Strip,
                        Text = receiptItem.Text,
                        Price = receiptItem.Price,
                        VAT = receiptItem.VAT,
                        Quantity = receiptItem.Quantity

                    };
                    orderItem.ReceiptItems.Add(receiptItemObj);
                }
            }

            //SHOULD FETCH RUN TIME GROUP BY VAT PERCENTAGE; WITH DELIVERY FEE + KORT BETALNINGS FEE
            _mPendingOrder.VATPercentage = 12;
            _mPendingOrder.VATAmount = _mPendingOrder.Total * 0.12m;
            return Ok(_mPendingOrder);
        }


        public string CleanApiString(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            else
                return value.Replace("null", "");
        }

        public class OrderStatusValue
        {
            public string token { get; set; }
            public string OrderGuid { get; set; }
            public int OrderStatus { get; set; }
            public string PickUpTime { get; set; }
            public string DeliveryTime { get; set; }
        }
        public class DeliveryDeniedValue
        {
            public string token { get; set; }
            public string OrderGuid { get; set; }
        }
    }
}