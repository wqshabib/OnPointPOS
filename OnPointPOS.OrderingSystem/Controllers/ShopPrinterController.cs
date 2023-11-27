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

namespace ML.WebAPI.Controllers
{

    //[RequireHttps]
    // [EnableCors(origins: "*", headers: "*", methods: "*")]
    //[Authorize]
    [RoutePrefix("api/ShopPrinter")]
    public class ShopPrinterController : ApiController
    {
        private List<PendingOrder> _mPendingOrderLists = new List<PendingOrder>();
        public ShopPrinterController()
        {
        }

       

        /// <summary>
        /// url:api/ShopPrinter/UpdateOrderStatus, Purpose: update the status,PickUpTime and DeliveryDateTime against any order, order guid is mandatory.
        /// </summary>
        /// <param name="OrderGuid"></param>
        /// <param name="orderStatus"></param>   
        /// <param name="PickUpTime"></param> 
        /// <param name="DeliveryDateTime"></param>     
        /// <returns></returns>

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("UpdateOrderStatus")]
        public IHttpActionResult UpdateOrderStatus(OrderStatusPrinterValue orderStatus)
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
            if ((int)DB.OrderStatusRepository.OrderStatus.Completed == orderStatus.OrderStatus || (int)DB.OrderStatusRepository.OrderStatus.ShopConfirmed == orderStatus.OrderStatus)
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
                DB.OrderStatusService.AddOrderStatus(order, OrderStatusRepository.OrderStatus.ShopAcknowledged, DB.OrderStatusRepository.ClientType.Printer);
            }
            if ((int)DB.OrderStatusRepository.OrderStatus.Delivered == orderStatus.OrderStatus || (int)DB.OrderStatusRepository.OrderStatus.Completed == orderStatus.OrderStatus ||(int)DB.OrderStatusRepository.OrderStatus.DriverConfirmed == orderStatus.OrderStatus || (int)DB.OrderStatusRepository.OrderStatus.ReadyToPickup == orderStatus.OrderStatus || (int)DB.OrderStatusRepository.OrderStatus.OnWay == orderStatus.OrderStatus)
            {
                if (!DB.OrderStatusService.HasOrderStatus(order.OrderGuid, DB.OrderStatusRepository.OrderStatus.Processing))
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
        /// url:'api/ShopPrinter/GetPendingOrders, Purpose=get the pending order.
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
            bool mergedApp = false;
            var shopConfig = tUser.tCompany.tShopConfig.FirstOrDefault();
            if (shopConfig != null && shopConfig.MergedApp == true)
                mergedApp = true;
            List<tOrder> orders;
            if (mergedApp)
                orders = new DB.OrderRepository().GetPendingOrders(tUser.CompanyGuid, tUser.tUserPrinterBridge.ToList()).ToList();
            else
                orders = new DB.OrderRepository().GetPrinterPendingOrders(tUser.CompanyGuid, tUser.tUserPrinterBridge.ToList()).ToList();

            

            _mPendingOrderLists.Clear();

            foreach (var order in orders)
            {
                var _mPendingOrder = new PendingOrder(order) {MergedApp = mergedApp};
                _mPendingOrderLists.Add(_mPendingOrder);
            }
            DB.OrderPrinterRepository orderPrinterRepository = new DB.OrderPrinterRepository();
            foreach (var item in tUser.tUserPrinterBridge.ToList())
            {
                DB.tOrderPrinter orderPrinter = orderPrinterRepository.GetByCompanyGuidAndOrderPrinterNo(tUser.CompanyGuid, item.tOrderPrinter.OrderPrinterNo);
                if (orderPrinter != null)
                {
                    // Keep printer alive, Add extra 2 minutes in case delay in API Request
                    orderPrinter.AliveDateTime = DateTime.Now.AddMinutes(2);
                    orderPrinter.AliveCount += 1;
                    orderPrinterRepository.Save();
                    // Log
                    new DB.OrderPrinterLogService().Log(orderPrinter.OrderPrinterGuid);
                }
            }

            return Ok(_mPendingOrderLists);
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
            var orders = new DB.OrderRepository().GetOrderHistoryCompleted(tUser.CompanyGuid, tUser.tUserPrinterBridge.ToList()).ToList();

            _mPendingOrderLists.Clear();

            foreach (var order in orders)
            {
                var _mPendingOrder = new PendingOrder(order);
                _mPendingOrderLists.Add(_mPendingOrder);
            }
            return Ok(_mPendingOrderLists);

        }

        /// <summary>
        /// url:'api/ShopPrinter/GetOrderDetail, Purpose=get all item detail against current order.
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
            var tUser = new DB.UserRepository().GetByUserGuid(usertoken.UserGuid);
            var shopConfig = tUser.tCompany.tShopConfig.FirstOrDefault();
            
            var _mPendingOrder = new PendingOrder(order)
            {
                MergedApp = (shopConfig != null && shopConfig.MergedApp == true),
                OrderItem = new List<OrderItem>()
            };

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
            return value.Replace("null", "");
        }

        public class OrderStatusPrinterValue
        {
            public string token { get; set; }
            public string OrderGuid { get; set; }
            public int OrderStatus { get; set; }
            public string PickUpTime { get; set; }
            public string DeliveryTime { get; set; }
        }
    }
}