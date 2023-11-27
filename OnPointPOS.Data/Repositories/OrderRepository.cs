using System;
using System.Collections.Generic;
using System.Linq;
using POSSUM.Model;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity.Migrations;
using System.Data.Entity;



namespace POSSUM.Data
{
    public class OrderRepository : GenericRepository<Order>, IDisposable
    {
        private readonly ApplicationDbContext db;

        public OrderRepository(ApplicationDbContext context) : base(context)
        {
            this.db = context;
        }

        public override void Add(Order entity)
        {

            base.Add(entity);
        }

        public OrderRepository() : base(new ApplicationDbContext())
        {
        }

        #region Save Order

        public Order SaveOrderMaster(Order order)
        {


            if (order.Id == default(Guid))
            {
                order.Id = Guid.NewGuid();
                var journal = new Journal
                {
                    ActionId = Convert.ToInt16(JournalActionCode.NewOrderEntry),
                    Created = DateTime.Now,
                    OrderId = order.Id,
                    UserId = order.UserId,
                    TerminalId = order.TerminalId
                };
                db.Journal.Add(journal);
            }

            if (string.IsNullOrEmpty(order.OrderNoOfDay))
            {
                var ordDate = DateTime.Now.Date;
                int lastNo = 0;
                var ord = db.OrderMaster.OrderByDescending(o => o.CreationDate).FirstOrDefault(o => o.OrderNoOfDay != "" && o.CreationDate >= ordDate);
                if (ord != null)
                {
                    if (ord.OrderNoOfDay != null)
                    {
                        string[] orNo = ord.OrderNoOfDay.Split('-');
                        if (orNo.Length > 1)
                            int.TryParse(orNo[1], out lastNo);
                    }

                }
                order.OrderNoOfDay = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day + "-" + (lastNo + 1);

            }

            db.OrderMaster.AddOrUpdate(order.GetFrom());
            db.SaveChanges();

            return order;


        }

        public Order SaveMqttOrderMaster(Order order)
        {
            if (order.Id == default(Guid))
            {
                order.Id = Guid.NewGuid();

                var journal = new Journal
                {
                    ActionId = Convert.ToInt16(JournalActionCode.NewOrderEntry),
                    Created = DateTime.Now,
                    OrderId = order.Id,
                    UserId = order.UserId,
                    TerminalId = order.TerminalId
                };

                db.Journal.Add(journal);
            }

            if (string.IsNullOrEmpty(order.OrderNoOfDay))
            {
                var ordDate = DateTime.Now.Date;
                int lastNo = 0;
                var ord = db.OrderMaster.OrderByDescending(o => o.CreationDate).FirstOrDefault(o => o.OrderNoOfDay != "" && o.CreationDate >= ordDate);
                if (ord != null)
                {
                    if (ord.OrderNoOfDay != null)
                    {
                        string[] orNo = ord.OrderNoOfDay.Split('-');
                        if (orNo.Length > 1)
                            int.TryParse(orNo[1], out lastNo);
                    }
                }

                order.OrderNoOfDay = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day + "-" + (lastNo + 1);
            }

            if (string.IsNullOrEmpty(order.Bong))
            {
                var bong = db.BongCounter.FirstOrDefault();
                int lastBong = 1;
                if (bong != null)
                {
                    lastBong = bong.Counter;
                    order.Bong = lastBong.ToString();
                    bong.Counter = lastBong + 1;

                    db.Entry(bong).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    order.Bong = lastBong.ToString();

                    BongCounter bongCounter = new BongCounter();
                    bongCounter.Counter = lastBong + 1;
                    bongCounter.BarCounter = 1;
                    db.BongCounter.Add(bongCounter);
                }
            }

            db.OrderMaster.AddOrUpdate(order.GetFrom());
            db.SaveChanges();

            return order;


        }

        public bool EditOrderMaster(Order order, int numberDevide)
        {

            var _order = db.OrderMaster.FirstOrDefault(o => o.Id == order.Id);

            if (string.IsNullOrEmpty(_order.OrderNoOfDay))
            {
                int lastNo = 0;
                DateTime toDate = DateTime.Now.Date;
                var ord = db.OrderMaster.OrderByDescending(o => o.CreationDate).FirstOrDefault(o => o.OrderNoOfDay != "" && o.CreationDate >= toDate);
                if (ord != null)
                {
                    string[] orNo = ord.OrderNoOfDay.Split('-');
                    if (orNo.Length > 1)
                        int.TryParse(orNo[1], out lastNo);
                }
                _order.OrderNoOfDay = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day + "-" + (lastNo + 1);
            }
            if (_order != null)
            {


                var lines = db.OrderDetail.Where(ol => ol.Active == 1 && ol.ItemType != ItemType.Grouped && ol.OrderId == order.Id).ToList();
                var sumAmmount = lines.Sum(s => s.GrossAmountDiscounted());

                sumAmmount = sumAmmount / numberDevide;// munir (split number)

                _order.Status = order.Status;
                _order.TableId = order.TableId;
                _order.OrderTotal = sumAmmount;
                _order.PaymentStatus = order.PaymentStatus;
                _order.InvoiceNumber = order.InvoiceNumber;
                _order.InvoiceDate = DateTime.Now;
                _order.InvoiceGenerated = order.InvoiceGenerated;
                _order.Updated = order.Updated;
                _order.Comments = order.Comments;
                _order.CustomerId = order.CustomerId;
                _order.OrderComments = order.OrderComments;
                //if (_order.Outlet == null)
                //    _order.Outlet = order.Outlet;
                //if (_order.TerminalId == null)
                //    _order.TerminalId = order.TerminalId;
                if (order.TableId > 0)
                    _order.Type = OrderType.TableOrder;
                else
                    _order.Type = order.Type;

                db.Entry(_order).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            return true;

        }

        public DepositHistory GetDepositHistoryForOrder(Guid orderId)
        {
            try
            {
                var depositHistory = db.DepositHistory.FirstOrDefault(a => a.OrderId == orderId);
                return depositHistory;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool SaveOrderLines(List<OrderLine> orderLines, Order order)
        {

            bool isMerge = false;
            if (orderLines.First().OrderId != order.Id)
                isMerge = true;
            decimal _total = db.OrderDetail.Where(o => o.Active == 1 && o.ItemType != ItemType.Grouped && o.IsCoupon != 1 && o.OrderId == order.Id).ToList().Sum(s => s.GrossAmountDiscounted());
            var ntotal = orderLines.Where(ol => ol.Active == 1 && ol.ItemType != ItemType.Grouped).ToList().Sum(s => s.GrossAmountDiscounted());


            foreach (var line in orderLines)
            {
                if (line.Id == default(Guid))
                {
                    line.Id = Guid.NewGuid();
                    int actionId;
                    string logmessage = "";
                    if (order.Status == OrderStatus.ReturnOrder)
                    {
                        actionId = Convert.ToInt16(JournalActionCode.ReturnItemAdded);
                        logmessage = "(" + line.ItemName + ")" + " - is add as return for order#: " + order.OrderNoOfDay + " |qty:" + line.Quantity + " unitprice: " + line.UnitPrice + " vat: " + Math.Round(line.VatAmount(), 2) + " vat%: " + Math.Round(line.TaxPercent, 2);
                    }

                    else
                    {
                        actionId = Convert.ToInt16(JournalActionCode.ItemAdded);
                        logmessage = "(" + line.ItemName + ")" + " - is added in cart for order#: " + order.OrderNoOfDay + " |qty:" + line.Quantity + " unitprice: " + line.UnitPrice + " vat: " + Math.Round(line.VatAmount(), 2) + " vat%: " + Math.Round(line.TaxPercent, 2);
                    }
                    var journal = new Journal
                    {
                        ActionId = actionId,
                        Created = DateTime.Now,
                        ItemId = line.ItemId,
                        OrderId = order.Id,
                        UserId = order.UserId,
                        TerminalId = order.TerminalId
                    };
                    db.Journal.Add(journal);
                }
                line.OrderId = order.Id;


                db.OrderDetail.AddOrUpdate(line.GetFrom());// new code here

                if (line.ItemDetails != null && line.ItemDetails.Count > 0)
                {
                    foreach (var innerLine in line.ItemDetails)
                    {
                        if (innerLine.Id == default(Guid))
                            innerLine.Id = Guid.NewGuid();
                        innerLine.OrderId = order.Id;
                        innerLine.ItemStatus = 3;

                        db.OrderDetail.AddOrUpdate(innerLine.GetFrom());


                    }
                }
                if (line.IngredientItems != null && line.IngredientItems.Count > 0)
                {
                    foreach (var _ingredientItem in line.IngredientItems)
                    {
                        if (_ingredientItem.Id == default(Guid))
                        {
                            _ingredientItem.Id = Guid.NewGuid();
                        }
                        _ingredientItem.GroupKey = line.Id;
                        _ingredientItem.OrderId = order.Id;

                        // _ingredientItem.Active = 0;
                        db.OrderDetail.AddOrUpdate(_ingredientItem.GetFrom());
                        ntotal += _ingredientItem.GrossAmountDiscounted();
                    }
                }



            }
            if (isMerge)
                order.OrderTotal = _total + ntotal;
            db.OrderMaster.AddOrUpdate(order.GetFrom());
            db.SaveChanges();
            return true;


        }
        public OrderLine SaveOrderLine(OrderLine orderLine, Order order)
        {
            bool isNew = false;

            if (orderLine.Id == default(Guid))
            {
                orderLine.Id = Guid.NewGuid();
                isNew = true;
            }
            if (orderLine.Direction == -1)
                isNew = true;
            orderLine.OrderId = order.Id;

            //  var ab = orderLine;

            db.OrderDetail.AddOrUpdate(orderLine.GetFrom());

            if (orderLine.ItemDetails != null && orderLine.ItemDetails.Count > 0)
            {
                foreach (var innerItem in orderLine.ItemDetails)
                {
                    if (innerItem.Id == default(Guid))
                        innerItem.Id = Guid.NewGuid();
                    innerItem.OrderId = order.Id;
                    var innerLine = innerItem;

                    db.OrderDetail.AddOrUpdate(innerLine.GetFrom());


                }
            }
            if (orderLine.IngredientItems != null && orderLine.IngredientItems.Count > 0)
            {
                foreach (var ingredientItem in orderLine.IngredientItems)
                {
                    if (ingredientItem.Id == default(Guid))
                        ingredientItem.Id = Guid.NewGuid();
                    ingredientItem.OrderId = order.Id;
                    ingredientItem.GroupKey = orderLine.Id;
                    var innerLine = ingredientItem;

                    db.OrderDetail.AddOrUpdate(innerLine.GetFrom());


                }
            }
            int actionId;
            string logmessage = "";
            if (order.Status == OrderStatus.ReturnOrder)
            {
                actionId = Convert.ToInt16(JournalActionCode.ReturnItemAdded);

                logmessage = "(" + orderLine.ItemName + ")" + " - is added as Return in cart for order#: " + order.OrderNoOfDay + " |qty:" + orderLine.Quantity + " unitprice: " + orderLine.UnitPrice + " vat: " + Math.Round(orderLine.VatAmount(), 2) + " vat%: " + orderLine.TaxPercent;
            }

            else
            {
                actionId = Convert.ToInt16(JournalActionCode.ItemAdded);
                logmessage = "(" + orderLine.ItemName + ")" + " - is added in cart for order#: " + order.OrderNoOfDay + " |qty:" + orderLine.Quantity + " unitprice: " + orderLine.UnitPrice + " vat: " + Math.Round(orderLine.VatAmount(), 2) + " vat%: " + orderLine.TaxPercent;

            }
            var journal = new Journal
            {
                ActionId = actionId,
                Created = DateTime.Now,
                ItemId = orderLine.ItemId,
                OrderId = order.Id,
                UserId = order.UserId,
                LogMessage = logmessage,
                TerminalId = order.TerminalId

            };
            if (isNew)
                db.Journal.Add(journal);

            db.SaveChanges();

            return orderLine;


        }

        public OrderLine UpdateOrderLine(OrderLine orderLine, Order order)
        {

            if (orderLine.Id == default(Guid))
            {
                orderLine.Id = Guid.NewGuid();
                int actionId = Convert.ToInt16(JournalActionCode.ItemAdded);

                string logmessage = "(" + orderLine.ItemName + ")" + " - is added in cart for order#: " + order.OrderNoOfDay + " |qty:" + orderLine.Quantity + " unitprice: " + orderLine.UnitPrice + " vat: " + Math.Round(orderLine.VatAmount(), 2) + " vat%: " + orderLine.TaxPercent;

                var journal = new Journal
                {
                    ActionId = actionId,
                    Created = DateTime.Now,
                    ItemId = orderLine.ItemId,
                    OrderId = order.Id,
                    UserId = order.UserId,
                    LogMessage = logmessage,
                    TerminalId = order.TerminalId
                };

                db.Journal.Add(journal);

            }
            orderLine.OrderId = order.Id;


            db.OrderDetail.AddOrUpdate(orderLine.GetFrom());

            if (orderLine.ItemDetails != null && orderLine.ItemDetails.Count > 0)
            {
                foreach (var innerItem in orderLine.ItemDetails)
                {
                    if (innerItem.Id == default(Guid))
                        innerItem.Id = Guid.NewGuid();
                    innerItem.OrderId = order.Id;
                    var innerLine = innerItem;

                    db.OrderDetail.AddOrUpdate(innerLine.GetFrom());


                }
            }
            if (orderLine.IngredientItems != null && orderLine.IngredientItems.Count > 0)
            {
                foreach (var ingredientItem in orderLine.IngredientItems)
                {
                    if (ingredientItem.Id == default(Guid))
                        ingredientItem.Id = Guid.NewGuid();
                    ingredientItem.OrderId = order.Id;
                    ingredientItem.GroupKey = orderLine.Id;
                    var innerLine = ingredientItem;

                    db.OrderDetail.AddOrUpdate(innerLine.GetFrom());


                }
            }

            db.SaveChanges();

            return orderLine;

        }

        public bool MoveOrderLines(List<OrderLine> orderLines, Order order)
        {

            bool isMerge = false;
            if (orderLines.First().OrderId != order.Id)
                isMerge = true;
            var oldOrderId = orderLines.FirstOrDefault().OrderId;
            var oldOrder = db.OrderMaster.FirstOrDefault(o => o.Id == oldOrderId);
            decimal _total = db.OrderDetail.Where(o => o.Active == 1 && o.ItemType != ItemType.Grouped && o.IsCoupon != 1 && o.OrderId == order.Id).ToList().Sum(s => s.GrossAmountDiscounted());
            var ntotal = orderLines.Where(ol => ol.Active == 1 && ol.ItemType != ItemType.Grouped).ToList().Sum(s => s.GrossAmountDiscounted());



            foreach (var line in orderLines)
            {

                string logmessage = "(" + line.ItemName + ")" + " - is moved from order#: " + oldOrder.OrderNoOfDay + " |qty:" + line.Quantity + " unitprice: " + line.UnitPrice + " vat: " + Math.Round(line.VatAmount(), 2) + " vat%: " + line.TaxPercent;


                var journal = new Journal
                {
                    ActionId = Convert.ToInt16(JournalActionCode.ItemMoved),
                    Created = DateTime.Now,
                    ItemId = line.ItemId,
                    OrderId = line.OrderId,
                    UserId = order.UserId,
                    LogMessage = logmessage,
                    TerminalId = order.TerminalId

                };
                db.Journal.Add(journal);

                line.OrderId = order.Id;
                logmessage = "(" + line.ItemName + ")" + " - is moved to order#: " + order.OrderNoOfDay + " |qty:" + line.Quantity + " unitprice: " + line.UnitPrice + " vat: " + Math.Round(line.VatAmount(), 2) + " vat%: " + line.TaxPercent;


                journal = new Journal
                {
                    ActionId = Convert.ToInt16(JournalActionCode.ItemAdded),
                    Created = DateTime.Now,
                    ItemId = line.ItemId,
                    OrderId = order.Id,
                    UserId = order.UserId,
                    LogMessage = logmessage
                };
                db.Journal.Add(journal);
                db.OrderDetail.AddOrUpdate(line.GetFrom());

                if (line.ItemDetails != null && line.ItemDetails.Count > 0)
                {
                    foreach (var innerLine in line.ItemDetails)
                    {
                        if (innerLine.Id == default(Guid))
                            innerLine.Id = Guid.NewGuid();
                        innerLine.OrderId = order.Id;
                        innerLine.ItemStatus = 3;

                        db.OrderDetail.AddOrUpdate(innerLine.GetFrom());

                    }
                }
                if (line.IngredientItems != null && line.IngredientItems.Count > 0)
                {
                    foreach (var _ingredientItem in line.IngredientItems)
                    {
                        if (_ingredientItem.Id == default(Guid))
                        {
                            _ingredientItem.Id = Guid.NewGuid();
                        }
                        _ingredientItem.GroupKey = line.Id;
                        _ingredientItem.OrderId = order.Id;

                        // _ingredientItem.Active = 0;
                        db.OrderDetail.AddOrUpdate(_ingredientItem.GetFrom());
                        ntotal += _ingredientItem.GrossAmountDiscounted();
                    }
                }



            }
            if (isMerge)
                order.OrderTotal = _total + ntotal;
            db.OrderMaster.AddOrUpdate(order.GetFrom());
            db.SaveChanges();
            return true;


        }
        public List<OrderLine> SaveOrderReturnOrder(Order order, List<OrderLine> orderDetail)
        {
            List<OrderLine> _returnList = new List<OrderLine>();

            order.CreationDate = DateTime.Now;

            foreach (var line in orderDetail)
            {
                line.Direction = -1;
                line.Id = Guid.NewGuid();
                line.OrderId = order.Id;

                db.OrderDetail.Add(line.GetFrom());
                // db.Entry(line.Product).State = System.Data.Entity.EntityState.Unchanged;
                _returnList.Add(line);


                if (line.IngredientItems != null && line.IngredientItems.Count > 0)
                {
                    var ingredients = line.IngredientItems.Where(c => c.GroupId == line.ItemId).ToList();

                    foreach (var ing in ingredients)
                    {
                        ing.Direction = -1;
                        ing.Id = Guid.NewGuid();
                        ing.OrderId = order.Id;

                        ing.GroupId = line.ItemId;
                        ing.GroupKey = line.Id;
                        ing.ItemDiscount = ing.ItemDiscount;

                        db.OrderDetail.Add(ing.GetFrom());
                        _returnList.Add(ing);

                    }
                }

            }


            if (string.IsNullOrEmpty(order.OrderNoOfDay))
            {
                var today = DateTime.Now.Date;
                var todayOrders = db.OrderMaster.Where(c => c.CreationDate > today).ToList();
                int count = todayOrders.Count + 1;
                order.OrderNoOfDay = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + "-" + count;
            }
            db.OrderMaster.Add(order.GetFrom());
            //  db.OrderDetail.AddRange(_returnList);

            db.SaveChanges();



            return _returnList;
        }

        public bool AddGredientsItems(OrderLine orderLine)
        {

            foreach (var _orderLine in orderLine.IngredientItems)
            {
                if (_orderLine.IngredientMode == "-")
                {
                    _orderLine.Quantity = orderLine.Quantity;
                    _orderLine.UnitPrice = 0;
                    _orderLine.DiscountedUnitPrice = 0;

                }


                if (_orderLine.Id == null || _orderLine.Id == default(Guid))
                {
                    _orderLine.OrderId = orderLine.OrderId;
                    _orderLine.Id = Guid.NewGuid();
                    _orderLine.GroupKey = orderLine.Id;
                    _orderLine.Description = _orderLine.Product.Description;
                    // _orderLine.Product = null;
                    _orderLine.Quantity = orderLine.Quantity;
                    db.OrderDetail.Add(_orderLine.GetFrom());
                    _orderLine.Id = _orderLine.Id;
                }
                else
                {
                    var _existingLine = db.OrderDetail.FirstOrDefault(ol => ol.Id == _orderLine.Id);
                    if (_existingLine == null)
                    {
                        _orderLine.OrderId = orderLine.OrderId;
                        _orderLine.Id = Guid.NewGuid();
                        _orderLine.GroupKey = orderLine.Id;
                        _orderLine.Description = _orderLine.Product.Description;
                        // _orderLine.Product = null;
                        _orderLine.Quantity = orderLine.Quantity;
                        db.OrderDetail.Add(_orderLine.GetFrom());
                        _orderLine.Id = _orderLine.Id;

                    }
                    else
                    {
                        _existingLine.Quantity = _orderLine.Quantity;
                        db.OrderDetail.AddOrUpdate(_existingLine);
                    }

                }
            }

            db.SaveChanges();
            return true;


        }
        public bool RemoveQuantity(OrderLine orderItem, string userId)
        {

            var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderItem.OrderId);
            var existingLine = db.OrderDetail.FirstOrDefault(o => o.Id == orderItem.Id);

            if (existingLine != null)
            {
                existingLine.Quantity = existingLine.Quantity - 1;
                db.OrderDetail.AddOrUpdate(existingLine);
                if (orderItem.ItemDetails != null)
                {
                    foreach (var itm in orderItem.ItemDetails)
                    {
                        var orderline = db.OrderDetail.FirstOrDefault(ol => ol.Id == itm.Id);
                        //due to not apply order in where cause active issue

                        if (orderline != null)
                        {
                            orderline.Quantity = existingLine.Quantity;
                            db.OrderDetail.AddOrUpdate(orderline);
                        }

                    }
                }
                if (orderItem.IngredientItems != null)
                {
                    foreach (var itm in orderItem.IngredientItems)
                    {
                        var orderline = db.OrderDetail.FirstOrDefault(ol => ol.Id == itm.Id);
                        //due to not apply order in where cause active issue

                        if (orderline != null)
                        {
                            orderline.Quantity = existingLine.Quantity;
                            db.OrderDetail.AddOrUpdate(orderline);
                        }

                    }
                }
                if (existingLine.ItemType == ItemType.Individual)
                {
                    var lineAudit = new OrderLine
                    {
                        Id = Guid.NewGuid(),
                        Direction = -1,
                        Quantity = 1,
                        ItemStatus = (int)OrderStatus.ReturnOrder,
                        OrderId = orderItem.OrderId,
                        Active = 0,
                        DiscountedUnitPrice = orderItem.DiscountedUnitPrice,
                        DiscountPercentage = orderItem.DiscountPercentage,
                        IsCoupon = orderItem.IsCoupon,
                        ItemComments = orderItem.ItemComments,
                        ItemDiscount = orderItem.ItemDiscount,
                        ItemId = orderItem.ItemId,
                        PurchasePrice = orderItem.PurchasePrice,
                        TaxPercent = orderItem.TaxPercent,
                        UnitPrice = orderItem.UnitPrice,
                        UnitsInPackage = orderItem.UnitsInPackage
                    };

                    db.OrderDetail.Add(lineAudit);
                }
                string logmessage = "(" + orderItem.ItemName + ") -is removed from cart for order#: " + order.OrderNoOfDay + " |qty:" + orderItem.Quantity + " unitprice: " + orderItem.UnitPrice + " vat: " + Math.Round(orderItem.VatAmount(), 2) + " vat%: " + orderItem.TaxPercent;

                var journal = new Journal
                {
                    ActionId = Convert.ToInt16(JournalActionCode.ItemDeleted),
                    Created = DateTime.Now,
                    ItemId = existingLine.ItemId,
                    OrderId = existingLine.OrderId,
                    UserId = userId,
                    LogMessage = logmessage,
                    TerminalId = order.TerminalId
                };
                db.Journal.Add(journal);

                db.SaveChanges();
            }

            return true;

        }
        public int DeleteItem(OrderLine orderItem, string userId)
        {
            int campaignBuyLimit = 0;

            var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderItem.OrderId);
            if (orderItem.Product.AskPrice == false && orderItem.Product.AskWeight == false)
            {
                var orderLines =
                    db.OrderDetail.Where(
                        ol => ol.Id == orderItem.Id && ol.OrderId == orderItem.OrderId).ToList();
                //due to not apply order in where cause active issue
                foreach (var orderLine in orderLines)
                {
                    orderLine.Active = 0;
                    db.OrderDetail.AddOrUpdate(orderLine);
                }
            }
            else
            {
                var orderLine = db.OrderDetail.FirstOrDefault(ol => ol.Id == orderItem.Id);
                //due to not apply order in where cause active issue
                if (orderLine != null)
                    orderLine.Active = 0;
            }

            if (orderItem.ItemDetails != null)
            {
                var orderLines =
                    db.OrderDetail.Where(ol => ol.GroupId == orderItem.ItemId && ol.OrderId == orderItem.OrderId).ToList();
                //due to not apply order in where cause active issue
                foreach (var orderLine in orderLines)
                {
                    orderLine.Active = 0;
                    db.OrderDetail.AddOrUpdate(orderLine);
                }
            }
            if (orderItem.IngredientItems != null)
            {
                var orderLines =
                    db.OrderDetail.Where(ol => ol.GroupId == orderItem.ItemId && ol.GroupKey == orderItem.Id && ol.OrderId == orderItem.OrderId).ToList();
                //due to not apply order in where cause active issue
                foreach (var orderLine in orderLines)
                {
                    orderLine.Active = 0;
                    db.OrderDetail.AddOrUpdate(orderLine);
                }
            }
            if (orderItem.CampaignId != 0)
            {
                var campaign = db.Campaign.FirstOrDefault(cmp => cmp.Id == orderItem.CampaignId);
                if (campaign != null)
                {
                    campaignBuyLimit = campaign.BuyLimit;
                    db.Campaign.AddOrUpdate(campaign);
                }
            }
            string logmessage = "(" + orderItem.ItemName + ") - is deleted from cart for order#: " + order.OrderNoOfDay + " |qty:" + orderItem.Quantity + " unitprice: " + orderItem.UnitPrice + " vat: " + Math.Round(orderItem.VatAmount(), 2) + " vat%: " + orderItem.TaxPercent;

            var journal = new Journal
            {
                ActionId = Convert.ToInt16(JournalActionCode.ItemDeleted),
                OrderId = orderItem.OrderId,
                ItemId = orderItem.ItemId,
                UserId = userId,
                Created = DateTime.Now,
                LogMessage = logmessage,
                TerminalId = order.TerminalId
            };
            db.Journal.Add(journal);
            db.SaveChanges();

            return campaignBuyLimit;
        }

        public List<Order> GetOrderByStatusFirstOrDefault(List<OrderStatus> orderStatus, Guid terminalId)
        {
            DateTime bt = DateTime.Now.AddMonths(-1);
            return db.OrderMaster.Where(o => orderStatus.Contains(o.Status) && o.TerminalId == terminalId && o.CreationDate > bt).ToList();
        }

        public bool RenameOrderTable(int tableId, string name)
        {

            var orders = db.OrderMaster.Where(o => o.TableId == tableId && o.Status == OrderStatus.AssignedKitchenBar).ToList();
            foreach (var order in orders)
            {
                order.Comments = name;
                db.OrderMaster.AddOrUpdate(order);
            }

            db.SaveChanges();
            return true;

        }

        public List<Order> GetOpenOrderByTable(int tableId)
        {

            List<Order> tableOrders = new List<Order>();
            db.Configuration.LazyLoadingEnabled = false;
            var selectedTable = db.FoodTable.FirstOrDefault(t => t.Id == tableId);
            var orders =
                db.OrderMaster.Include("OrderLines.Product").Where(c => c.TableId == tableId && c.Status == OrderStatus.AssignedKitchenBar).ToList();
            if (orders != null)
                foreach (var order in orders)
                {

                    var _orderLines = order.OrderLines.Where(ol => ol.Active == 1).ToList().Select(od => new OrderLine(od)).ToList();
                    if (_orderLines.Count == 0)
                        continue;
                    if (order != null)
                    {


                        var orderLines = _orderLines.Where(ol => ol.ItemType == ItemType.Individual && ol.GroupId == default(Guid)).ToList();


                        foreach (var line in orderLines)
                        {
                            if (line.ItemType == ItemType.Individual)
                            {
                                line.IngredientItems = _orderLines.Where(o => o.GroupId == line.ItemId && o.GroupKey == line.Id && o.ItemType == ItemType.Ingredient).ToList();

                            }

                            if (line.ItemType == ItemType.Grouped)
                            {
                                line.ItemDetails = _orderLines.Where(o => o.GroupId == line.ItemId).ToList();

                                orderLines.Add(line);

                            }

                        }
                        order.OrderLines = orderLines;
                        tableOrders.Add(order);
                    }
                }
            return tableOrders.OrderByDescending(c => c.CreationDate).ToList();


        }
        public List<Order> GetOpenTableOrders(Guid terminalId)
        {
            db.Configuration.LazyLoadingEnabled = false;
            var orders =
                db.OrderMaster.Include("OrderLines.Product").Where(
                    c =>
                        c.TableId > 0 && c.Status == OrderStatus.AssignedKitchenBar &&
                        c.TerminalId == terminalId).ToList();
            if (orders != null)
                foreach (var order in orders)
                {
                    var _orderLines = order.OrderLines.ToList().Select(od => new OrderLine(od)).ToList();
                    if (order != null)
                    {


                        var orderLines = _orderLines.Where(ol => ol.ItemType == ItemType.Individual && ol.GroupId == default(Guid)).ToList();


                        foreach (var line in _orderLines)
                        {
                            if (line.ItemType == ItemType.Individual)
                            {
                                line.IngredientItems = _orderLines.Where(o => o.GroupId == line.ItemId && o.GroupKey == line.Id && o.ItemType == ItemType.Ingredient).ToList();

                            }

                            if (line.ItemType == ItemType.Grouped)
                            {
                                line.ItemDetails = _orderLines.Where(o => o.GroupId == line.ItemId).ToList();

                                orderLines.Add(line);

                            }

                        }
                        order.OrderLines = orderLines;
                    }
                }
            return orders;

        }
        public bool SetOrderStatus(Guid orderId, OrderStatus orderStatus, string userId)
        {

            var _order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
            if (_order != null)
            {
                _order.Status = orderStatus;
                db.Entry(_order).State = System.Data.Entity.EntityState.Modified;
                // db.Entry(_order.Outlet).State = System.Data.Entity.EntityState.Unchanged;
                if (orderStatus == OrderStatus.OrderCancelled)
                {
                    var journal = new Journal
                    {
                        OrderId = orderId,
                        Created = DateTime.Now,
                        UserId = userId,
                        ActionId = Convert.ToInt16(JournalActionCode.OrderCancelled),
                        TerminalId = _order.TerminalId

                    };
                    db.Journal.Add(journal);
                }
                db.SaveChanges();
            }
            return true;

        }

        public bool UpdateBongName(Guid orderId, string name)
        {
            var _order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
            _order.Comments = name;
            db.Entry(_order).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return true;
        }
        public bool UpdateBongNo(Guid orderId, int dailyBongCounter, bool isDailyBongCounter)
        {

            var bong = db.BongCounter.FirstOrDefault();
            var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
            if (bong != null)
            {
                order.Bong = bong.Counter.ToString();
                int lastBong = bong.Counter + 1;
                bong.Counter = lastBong;
                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                db.Entry(bong).State = System.Data.Entity.EntityState.Modified;

            }
            if (isDailyBongCounter)
            {
                var _bong = db.BongCounter.FirstOrDefault(b => b.Id == 2);
                if (_bong != null)
                {
                    order.DailyBong = dailyBongCounter.ToString();
                    _bong.Counter = dailyBongCounter + 1;
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(_bong).State = System.Data.Entity.EntityState.Modified;
                }
            }
            db.SaveChanges();
            return true;

        }

        public bool UpdateOrderCustomerInfo(Guid orderId, Customer customer, string orderComments)
        {
            var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
            if (order != null && customer != null)
            {
                order.CustomerId = customer.Id;
                if (! string.IsNullOrEmpty(orderComments))
                { 
                    order.OrderComments = orderComments;
                }
                if (order.Status != OrderStatus.ReturnOrder)
                    order.Status = OrderStatus.Completed;
                db.SaveChanges();

            }
            return true;

        }

        public bool HasDiscountAvailable(Guid orderId)
        {
            try
            {
                var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);

                if (order != null)
                {
                    var lines = db.OrderDetail.Where(ol => ol.OrderId == orderId && ol.ItemType == 0 && !ol.Product.DiscountAllowed && ol.Active == 1).ToList();
                    return lines.Count == 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public decimal GetOrderTotal(Guid orderId)
        {
            try
            {
                var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);

                if (order != null)
                {
                    var lines = db.OrderDetail.Where(ol => ol.OrderId == orderId && ol.ItemType == 0 && ol.Product.DiscountAllowed && ol.Active == 1).ToList();
                    var total = lines.Sum(s => s.GrossAmount());

                    return total;
                }

                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public decimal UpdateOrderDiscount(Guid orderId, decimal amount, bool percentage, string userId)
        {
            decimal orderTotal = 0;

            var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);

            if (order != null)
            {
                var lines = db.OrderDetail.Where(ol => ol.OrderId == orderId && ol.ItemType == 0 && ol.Product.DiscountAllowed && ol.Active == 1).ToList();
                var total = lines.Sum(s => s.GrossAmount());

                // var pp= amount / total * 100;
                decimal percent = 0;
                if (percentage)
                {
                    var totaldiscount = (total / 100) * amount;
                    percent = totaldiscount / total * 100;
                }
                else
                    percent = amount / total * 100;
                var orderDetails = db.OrderDetail.Where(ol => ol.OrderId == orderId && ol.ItemType != ItemType.Grouped && ol.Active == 1).ToList();
                foreach (var orderDetail in lines)
                {
                    var grossTotal = orderDetail.GrossAmount();
                    orderDetail.ItemDiscount = Math.Round((grossTotal / 100) * percent, 2);
                    db.Entry(orderDetail).State = System.Data.Entity.EntityState.Modified;
                }
                var totalDiscount = lines.Sum(s => s.ItemDiscount);
                string logmessage = "Dicount given on  order#: " + order.OrderNoOfDay + " |Amount:" + Math.Round(totalDiscount, 2);

                var journal = new Journal
                {
                    ActionId = Convert.ToInt16(JournalActionCode.ItemDeleted),
                    OrderId = order.Id,
                    UserId = userId,
                    Created = DateTime.Now,
                    LogMessage = logmessage,
                    TerminalId = order.TerminalId
                };
                db.Journal.Add(journal);
                orderTotal = orderDetails.Sum(ol => ol.GrossAmountDiscounted());

                order.OrderTotal = orderTotal;
                // order.OrderLines = null;

                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return orderTotal;
        }


        public decimal CancelOrderDiscount(Guid orderId)
        {
            decimal orderTotal = 0;

            var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);

            if (order != null)
            {
                var lines = db.OrderDetail.Where(ol => ol.OrderId == orderId).ToList();

                foreach (var orderDetail in lines)
                {
                    orderDetail.ItemDiscount = 0;
                    orderDetail.DiscountPercentage = 0;
                    db.Entry(orderDetail).State = System.Data.Entity.EntityState.Modified;
                }
                orderTotal = lines.Sum(ol => ol.GrossAmountDiscounted());

                order.OrderTotal = orderTotal;
                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }


            return orderTotal;
        }
        public bool CancelOrder(Guid orderId, string userId)
        {

            var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.Status = OrderStatus.OrderCancelled;
                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                //context.Entry(order.Outlet).State = System.Data.Entity.EntityState.Unchanged;
                var journal = new Journal
                {
                    ActionId = Convert.ToInt16(JournalActionCode.OrderCancelled),
                    OrderId = orderId,
                    Created = DateTime.Now,
                    UserId = userId,
                    TerminalId = order.TerminalId
                };
                db.Journal.Add(journal);
                db.SaveChanges();
            }
            return true;
        }

        public bool SaveVoucherTransaction(VoucherTransaction transaction, string productSKU)
        {
            var product = db.Product.First(p => p.SKU == productSKU);
            transaction.Id = Guid.NewGuid();
            transaction.Product = product;
            db.VoucherTransaction.Add(transaction);
            db.SaveChanges();
            return true;
        }
        public bool SaveOrderPayment(Payment payment, decimal roundAmount, string userId)
        {

            var order = db.OrderMaster.First(o => o.Id == payment.OrderId);
            var ordertotal = order.OrderTotal;
            decimal paid = 0;
            var payments = db.Payment.AsQueryable().Where(p => p.OrderId == payment.OrderId && p.Direction == 1 && payment.TypeId != 7).ToList();
            if (payments.Count > 0)
            {
                paid = payments.Sum(p => p.PaidAmount);
                if (paid >= order.OrderTotal)
                    return true;
            }

            //var alreadyentry = paymentRepo.FirstOrDefault(p => p.Order.Id == payment.OrderId && p.PaymentType.Id == payment.TypeId && p.PaidAmount == ordertotal);
            //if (alreadyentry != null)
            //    return true;
            if (payment.TypeId == 1 || payment.TypeId == 10)
                order.RoundedAmount = roundAmount;
            payment.OrderId = order.Id;
            payment.Id = Guid.NewGuid();
            db.Payment.Add(payment);
            paid = paid + payment.PaidAmount;
            if (paid < ordertotal)
                order.PaymentStatus = 2;
            if (payment.TypeId == 0 || payment.TypeId == 1 || payment.TypeId == 2 || payment.TypeId == 4 || payment.TypeId == 7 || payment.TypeId == 9 || payment.TypeId == 10)
            {
                //0 coupon, 1 cash, 2 account, 3 Gifft, 4 CreditCard, 5 DebitCard, 7 cashback, 9 mobile, 10 swish
                int actionId = payment.TypeId == 0 ? Convert.ToInt16(JournalActionCode.OrderCouponPayment) : payment.TypeId == 1 ? Convert.ToInt16(JournalActionCode.OrderCashPayment) : payment.TypeId == 2 ? Convert.ToInt16(JournalActionCode.OrderAccountPayment) : payment.TypeId == 4 ? Convert.ToInt16(JournalActionCode.OrderCreditcardPayment) : payment.TypeId == 7 ? Convert.ToInt16(JournalActionCode.OrderReturnCashPayment) : payment.TypeId == 9 ? Convert.ToInt16(JournalActionCode.OrderMobileTerminalPayment) : Convert.ToInt16(JournalActionCode.OrderSwishPayment);
                var journal = new Journal
                {
                    ActionId = actionId,
                    Created = DateTime.Now,
                    OrderId = order.Id,
                    UserId = userId,
                    TerminalId = order.TerminalId
                };
                db.Journal.Add(journal);
            }
            db.OrderMaster.AddOrUpdate(order);

            db.SaveChanges();
            return true;

        }

        public bool SaveUserOrder(Order order)
        {
            if (order.UserOrder != null)
            {
                var dbUserOrder = db.UserOrder.FirstOrDefault(obj => obj.OrderId == order.UserOrder.OrderId);

                if (dbUserOrder == null)
                {
                    db.UserOrder.Add(order.UserOrder);
                    db.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        #endregion


        public Order GetOrderMasterDetailById(Guid orderId)
        {
            var orderModel = new Order();


            db.Configuration.LazyLoadingEnabled = false;
            orderModel = db.OrderMaster.Include("OrderLines.Product").Include("Payments").Include("Receipts").Include("Outlet").FirstOrDefault(o => o.Id == orderId);
            if (orderModel == null)
                return null;
            var table = new FoodTable();
            var cutomer = new Customer();
            if (orderModel.TableId != 0)
                table = db.FoodTable.FirstOrDefault(t => t.Id == orderModel.TableId);
            if (orderModel.CustomerId != default(Guid))
                cutomer = db.Customer.FirstOrDefault(t => t.Id == orderModel.CustomerId);
            if (orderModel.OrderLines != null)
            {
                foreach (var ordLine in orderModel.OrderLines.Where(od => od.Active == 1))
                    ordLine.Order = null;
            }
            if (orderModel.Payments != null && orderModel.Payments.Count > 0)
                foreach (var payment in orderModel.Payments)
                    payment.Order = null;
            if (orderModel.Receipts != null && orderModel.Receipts.Count > 0)
                foreach (var receipt in orderModel.Receipts)
                    receipt.Order = null;
            orderModel.SelectedTable = table != null ? new FoodTable { Id = table.Id, Name = table.Name } : null;
            orderModel.Customer = cutomer;

            if (orderModel.OrderLines != null)
            {
                var _orderLines = orderModel.OrderLines.Where(od => od.Active == 1).ToList().Select(od => new OrderLine(od)).ToList();

                if (orderModel != null)
                {


                    var orderLines = _orderLines.Where(ol => ol.ItemType == ItemType.Individual && ol.GroupId == default(Guid)).ToList();


                    foreach (var line in _orderLines)
                    {
                        if (line.ItemType != ItemType.Ingredient)
                        {
                            line.IngredientItems = _orderLines.Where(o => o.GroupId == line.ItemId && o.GroupKey == line.Id && o.ItemType == ItemType.Ingredient).ToList();

                        }

                        if (line.ItemType == ItemType.Grouped)
                        {
                            line.ItemDetails = _orderLines.Where(o => o.GroupId == line.ItemId).ToList();

                            orderLines.Add(line);

                        }

                    }
                    orderModel.OrderLines = orderLines;
                }
            }
            if (orderModel.Receipts != null && orderModel.Receipts.Count > 0)
                orderModel.Receipt = orderModel.Receipts.FirstOrDefault();
            return orderModel;

        }

        public Order GetOrderMasterDetailByIdAddToCart(Guid orderId)
        {
            var orderModel = new Order();


            db.Configuration.LazyLoadingEnabled = false;
            orderModel = db.OrderMaster.Include("OrderLines.Product").FirstOrDefault(o => o.Id == orderId);
            if (orderModel == null)
                return null;
            var table = new FoodTable();
            var cutomer = new Customer();
            if (orderModel.TableId != 0)
                table = db.FoodTable.FirstOrDefault(t => t.Id == orderModel.TableId);
            if (orderModel.CustomerId != default(Guid))
                cutomer = db.Customer.FirstOrDefault(t => t.Id == orderModel.CustomerId);
            if (orderModel.OrderLines != null)
            {
                foreach (var ordLine in orderModel.OrderLines.Where(od => od.Active == 1))
                    ordLine.Order = null;
            }
            if (orderModel.Payments != null && orderModel.Payments.Count > 0)
                foreach (var payment in orderModel.Payments)
                    payment.Order = null;
            if (orderModel.Receipts != null && orderModel.Receipts.Count > 0)
                foreach (var receipt in orderModel.Receipts)
                    receipt.Order = null;
            orderModel.SelectedTable = table != null ? new FoodTable { Id = table.Id, Name = table.Name } : null;
            orderModel.Customer = cutomer;

            if (orderModel.OrderLines != null)
            {
                var _orderLines = orderModel.OrderLines.Where(od => od.Active == 1).ToList().Select(od => new OrderLine(od)).ToList();

                if (orderModel != null)
                {


                    var orderLines = _orderLines.Where(ol => ol.ItemType == ItemType.Individual && ol.GroupId == default(Guid)).ToList();


                    foreach (var line in _orderLines)
                    {
                        if (line.ItemType != ItemType.Ingredient)
                        {
                            line.IngredientItems = _orderLines.Where(o => o.GroupId == line.ItemId && o.GroupKey == line.Id && o.ItemType == ItemType.Ingredient).ToList();

                        }

                        if (line.ItemType == ItemType.Grouped)
                        {
                            line.ItemDetails = _orderLines.Where(o => o.GroupId == line.ItemId).ToList();

                            orderLines.Add(line);

                        }

                    }
                    orderModel.OrderLines = orderLines;
                }
            }
            if (orderModel.Receipts != null && orderModel.Receipts.Count > 0)
                orderModel.Receipt = orderModel.Receipts.FirstOrDefault();
            return orderModel;

        }

        public List<OrderLine> GetOrderLinesById(Guid orderId)
        {
            try
            {
                var order = new Order();
                db.Entry(order).State = EntityState.Detached;// getting for refresh data

                var orderLines = new List<OrderLine>();

                //  db.Configuration.LazyLoadingEnabled = false;
                order.OrderLines = db.OrderDetail.Include("Product").Where(o => o.OrderId == orderId && o.Active == 1 && o.IsCoupon != 1).ToList().Select(od => new OrderLine(od)).ToList();


                orderLines = order.OrderLines.Where(ol => ol.ItemType == ItemType.Individual && ol.GroupId == default(Guid)).ToList();


                foreach (var line in order.OrderLines)
                {
                    if (line.ItemType != ItemType.Ingredient)
                    {
                        line.IngredientItems = order.OrderLines.Where(o => o.GroupId == line.ItemId && o.GroupKey == line.Id && o.ItemType == ItemType.Ingredient).ToList();

                    }

                    if (line.ItemType == ItemType.Grouped)
                    {
                        line.ItemDetails = order.OrderLines.Where(o => o.GroupId == line.ItemId).ToList();

                        orderLines.Add(line);

                    }

                }
                return orderLines;

            }
            catch (Exception ex)
            {

                // LogWriter.LogWrite(ex);
                return new List<OrderLine>();
            }


        }


        public List<Order> UpdateHistoryGrid(DateTime dateFrom, DateTime dateTo, Guid terminalId)
        {

            var orders = new List<Order>();
            var data = (from ord in db.OrderMaster.Where(o => o.InvoiceGenerated == 1 && 
                        (o.Status == OrderStatus.Completed || o.Status == OrderStatus.ReturnOrder)
                        && o.TerminalId == terminalId && (o.InvoiceDate >= dateFrom && o.InvoiceDate <= dateTo))
                        join recipt in db.Receipt on ord.Id equals recipt.OrderId
                        select new
                        {
                            Id = ord.Id,
                            InvoiceNumber = recipt.ReceiptNumber.ToString(),
                            OrderComments = ord.Comments,
                            OrderNoOfDay = ord.OrderNoOfDay,
                            InvoiceDate = ord.InvoiceDate,
                            OrderTotal = ord.OrderTotal,
                            Status = ord.Status,
                            InvoiceGenerated = ord.InvoiceGenerated
                        }).ToList();

            if (data != null && data.Count > 0)
                orders = data.Select(ord => new Order
                {
                    Id = ord.Id,
                    InvoiceNumber = ord.InvoiceNumber,
                    OrderComments = ord.OrderComments,
                    OrderNoOfDay = ord.OrderNoOfDay,
                    InvoiceDate = ord.InvoiceDate,
                    OrderTotal = ord.OrderTotal,
                    Status = ord.Status,
                    InvoiceGenerated = ord.InvoiceGenerated
                }).ToList();


            return orders;
        }


        public Order GetFirstOrDefault()
        {
            return db.OrderMaster.FirstOrDefault();
        }
        public Order GetOrderMaster(Guid orderId)
        {
            return db.OrderMaster.FirstOrDefault(ot => ot.Id == orderId);
        }
        public Receipt GetOrderReceipt(Guid orderId)
        {
            return db.Receipt.FirstOrDefault(ot => ot.OrderId == orderId);
        }
        public List<Order> GetOpenOrdersOnTable(int tableId)
        {
            db.Configuration.LazyLoadingEnabled = false;
            return
                db.OrderMaster.Where(o => o.TableId == tableId && o.Status == OrderStatus.AssignedKitchenBar)
                    .ToList();
        }

        public List<Order> GetCustomersOrders(Guid customerId)
        {
            db.Configuration.LazyLoadingEnabled = false;
            return
                db.OrderMaster.Where(o => o.CustomerId == customerId)
                    .ToList();
        }
        public Guid GenerateCustomerInvoice(Guid OrderId, Guid customerId)
        {
            var order = db.OrderMaster.FirstOrDefault(o => o.Id == OrderId);
            long invoiceNo = 0;

            var invoiceCounter = db.InvoiceCounter.FirstOrDefault(inc => inc.Id == 2);

            if (invoiceCounter != null)
            {
                invoiceNo = Convert.ToInt64(invoiceCounter.LastNo);
                invoiceCounter.LastNo = (invoiceNo + 1).ToString();
            }
            else
            {
                invoiceCounter = new InvoiceCounter
                {
                    Id = 2,
                    LastNo = "1",
                    InvoiceType = InvoiceType.Customer
                };
                db.InvoiceCounter.Add(invoiceCounter);
            }
            invoiceNo = invoiceNo + 1;
            Guid invoiceId = Guid.NewGuid();
            var invoice = new CustomerInvoice
            {
                Id = invoiceId,
                InvoiceNumber = invoiceNo,
                CreationDate = DateTime.Now,
                InvoiceTotal = order.OrderTotal,
                Remarks = order.OrderComments,
                CustomerId = customerId
            };
            db.CustomerInvoice.Add(invoice);

            order.CustomerInvoiceId = invoiceId;
            db.SaveChanges();
            return invoiceId;
        }
        public Guid GenerateCustomerInvoice(List<Order> orders, Guid customerId, string remanrks)
        {
            decimal invoiceTotal = orders.Sum(o => o.OrderTotal);
            long invoiceNo = 0;
            var invoiceCounter = db.InvoiceCounter.FirstOrDefault(inc => inc.Id == 2);

            if (invoiceCounter != null)
            {
                invoiceNo = Convert.ToInt64(invoiceCounter.LastNo);
                invoiceCounter.LastNo = (invoiceNo + 1).ToString();
            }
            else
            {
                invoiceCounter = new InvoiceCounter
                {
                    Id = 2,
                    LastNo = "1",
                    InvoiceType = InvoiceType.Customer
                };
                db.InvoiceCounter.Add(invoiceCounter);
            }
            invoiceNo = invoiceNo + 1;
            Guid invoiceId = Guid.NewGuid();
            var invoice = new CustomerInvoice
            {
                Id = invoiceId,
                InvoiceNumber = invoiceNo,
                CreationDate = DateTime.Now,
                InvoiceTotal = invoiceTotal,
                Remarks = remanrks,
                CustomerId = customerId
            };
            db.CustomerInvoice.Add(invoice);
            foreach (var order in orders)
            {
                var _order = db.OrderMaster.FirstOrDefault(o => o.Id == order.Id);
                _order.CustomerInvoiceId = invoiceId;
            }
            db.SaveChanges();
            return invoiceId;
        }

        public CustomerInvoiceModel GetCustomerInvoice(Guid InvoiceId)
        {

            CustomerInvoiceModel model = new CustomerInvoiceModel();
            db.Configuration.LazyLoadingEnabled = false;
            var invoice = db.CustomerInvoice.FirstOrDefault(inv => inv.Id == InvoiceId);
            if (invoice != null)
            {
                model.Invoice = invoice;
                var customer = db.Customer.FirstOrDefault(cust => cust.Id == invoice.CustomerId);
                if (customer != null)
                    model.Customer = customer;
                //Detail


                var orderLines = (from ordLine in db.OrderDetail.Include("Product").Where(o => o.Active == 1 && o.IsCoupon != 1)
                                  join ord in db.OrderMaster on ordLine.OrderId equals ord.Id
                                  where ord.CustomerInvoiceId == InvoiceId
                                  select ordLine).ToList();



                var groups = orderLines.GroupBy(g => new { g.ItemId, g.UnitPrice }).ToList();
                List<OrderLine> lines = new List<OrderLine>();
                foreach (var group in groups)
                {
                    var ordLine = group.First();
                    var grossTotal = group.Sum(s => s.GrossAmount());
                    var netTotal = group.Sum(s => s.NetAmount());
                    var qty = group.Sum(s => (s.Direction * s.Quantity));
                    lines.Add(ordLine);
                }

                model.OrderDetails = lines.OrderBy(o => o.ItemName).ToList();
            }

            return model;

        }

        public List<CustomerInvoice> SearchInvoices(DateTime startDate, DateTime endDate, string search)
        {
            startDate = startDate.Date;

            endDate = endDate.Date.AddDays(1);

            endDate = endDate.AddMinutes(-1);

            var invoices = db.CustomerInvoice.ToList().Where(i => i.CreationDate >= startDate && i.CreationDate <= endDate).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                invoices = invoices.Where(i => i.InvoiceNumber.ToString().Contains(search)).ToList();
            }

            return invoices;
        }

        public List<Order> SearchOrders(DateTime startDate, DateTime endDate, string search)
        {
            startDate = startDate.Date;

            endDate = endDate.Date.AddDays(1);

            endDate = endDate.AddMinutes(-1);

            var guid = Guid.Empty;

            var orders = base.Where(i => i.InvoiceDate >= startDate && i.InvoiceDate <= endDate && i.CustomerId == guid && i.InvoiceGenerated == 1).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                orders = orders.Where(i => i.InvoiceNumber.Contains(search)).ToList();
            }

            return orders;
        }

        public List<Order> GetPendingOrders(OrderType type, Guid terminalId)
        {
            db.Configuration.LazyLoadingEnabled = false;
            var orders =
                    db.OrderMaster.Include(a=>a.Payments).Where(
                            c =>
                                c.TableId == 0 &&
                                (c.Status != OrderStatus.Completed && c.Status != OrderStatus.OrderCancelled && c.Status != OrderStatus.Rejected && c.Status != OrderStatus.New) &&
                                c.PaymentStatus == 0 &&
                                c.TerminalId == terminalId)
                        .OrderByDescending(o => o.CreationDate).ToList();
            foreach (var order in orders)
            {
                decimal paid = 0;
                //var payments = db.Payment.Where(p => p.OrderId == order.Id).ToList();
                if (order.Payments != null && order.Payments.Count > 0)
                    paid = order.Payments.Sum(p => p.PaidAmount);
                order.BalanceAmount = order.OrderTotal - paid;

            }
            return orders;
        }
        public List<Order> GetPendingOrdersByUser(string userId)
        {
            db.Configuration.LazyLoadingEnabled = false;
            return
                 db.OrderMaster.Where(
                         x =>
                             x.UserId == userId && x.InvoiceGenerated != 1 &&
                             x.Status != OrderStatus.Completed).ToList();
        }

        public List<Order> GetOpenOrders(Guid TerminalId)
        {
            return Where(
                         c =>
                             c.TableId > 0 && c.Status == OrderStatus.AssignedKitchenBar
                             && c.TerminalId == TerminalId).ToList();
        }

        public List<OrderLine> DeleteLineItems(Guid orderId, Guid lineId, List<OrderLine> details)
        {

            List<OrderLine> lines;
            var orderLine = db.OrderDetail.FirstOrDefault(ol => ol.Id == lineId);
            if (orderLine != null)
            {
                orderLine.Active = 0;
                db.Entry(orderLine).State = System.Data.Entity.EntityState.Modified;
                db.Entry(orderLine.Product).State = System.Data.Entity.EntityState.Unchanged;
                db.SaveChanges();
            }

            var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
            lines = db.OrderDetail.Where(o => o.OrderId == orderId && o.Active == 1).ToList();
            if (lines.Count == 0)
            {
                if (order != null)
                {
                    order.Status = OrderStatus.OrderCancelled;
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(order.Outlet).State = System.Data.Entity.EntityState.Unchanged;
                }
            }
            else
            {
                decimal total = 0;
                decimal tax = 0;
                foreach (var lineItem in lines)
                {
                    if (lineItem.Active == 1 && lineItem.IsCoupon != 1)
                    {
                        tax += lineItem.VatAmount();
                        total += lineItem.GrossAmount();
                        db.Entry(lineItem).State = System.Data.Entity.EntityState.Modified;
                        db.Entry(lineItem.Product).State = System.Data.Entity.EntityState.Unchanged;
                    }
                }
                decimal ordertotal = total + tax;
                if (order != null) order.OrderTotal = ordertotal;
            }
            db.SaveChanges();

            return lines.Count == 0 ? new List<OrderLine>() : details;
        }

        public bool SplitOrder(Order order, List<OrderLine> lines, int number, string userId)
        {
            var orderDetails = lines.ToList();

            if (order != null)
            {
                var ordDate = DateTime.Now.Date;
                int lastNo = 0;
                var ord =
                    db.OrderMaster.OrderByDescending(o => o.CreationDate)
                        .FirstOrDefault(o => o.OrderNoOfDay != "" && o.CreationDate >= ordDate);
                string[] orNo = ord?.OrderNoOfDay.Split('-');
                if (orNo?.Length > 1)
                    int.TryParse(orNo[1], out lastNo);

                decimal divedSum = order.OrderTotal / number;
                for (int i = 1; i < number; i++)
                {
                    string orderNoOfDay = DateTime.Now.Year + DateTime.Now.Month.ToString() + DateTime.Now.Day +
                                          "-" + (lastNo + 1);
                    lastNo++;
                    var neworder = new Order
                    {
                        Id = Guid.NewGuid(),
                        CreationDate = DateTime.Now,
                        TableId = order.TableId,
                        Status = order.Status,
                        PaymentStatus = order.PaymentStatus,
                        OrderLines = order.OrderLines,
                        OrderNoOfDay = orderNoOfDay,
                        ShiftNo = order.ShiftNo,
                        ShiftOrderNo = order.ShiftOrderNo,
                        OrderTotal = divedSum,
                        TaxPercent = order.TaxPercent,
                        UserId = userId,
                        Updated = order.Updated,
                        OutletId = ord.OutletId,
                        TerminalId = order.TerminalId,
                        TrainingMode = order.TrainingMode,
                        Type = order.Type,
                        Comments = order.Comments
                    };

                    db.OrderMaster.Add(neworder);

                    foreach (var newLine in orderDetails)
                    {

                        newLine.Id = Guid.NewGuid();
                        newLine.Quantity = newLine.Quantity / number;
                        newLine.OrderId = neworder.Id;
                        db.OrderDetail.Add(newLine);
                        db.Entry(newLine.Product).State = System.Data.Entity.EntityState.Unchanged;
                        if (newLine.ItemDetails != null)
                        {
                            foreach (var newinnerLine in newLine.ItemDetails)
                            {

                                newinnerLine.Id = Guid.NewGuid();
                                newinnerLine.Quantity = newLine.Quantity;
                                newinnerLine.OrderId = neworder.Id;
                                db.OrderDetail.Add(newinnerLine);
                                db.Entry(newinnerLine.Product).State = System.Data.Entity.EntityState.Unchanged;
                            }
                            foreach (var newinnerLine in newLine.IngredientItems) // spliting the ingredients
                            {

                                newinnerLine.Id = Guid.NewGuid();
                                newinnerLine.Quantity = newLine.Quantity;
                                newinnerLine.OrderId = neworder.Id;
                                db.OrderDetail.Add(newinnerLine);
                                db.Entry(newinnerLine.Product).State = System.Data.Entity.EntityState.Unchanged;
                            }
                        }
                    }
                }
                foreach (var line in orderDetails)
                {
                    var newLine = db.OrderDetail.First(o => o.Id == line.Id);
                    newLine.Quantity = newLine.Quantity / number;
                    db.Entry(newLine).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(newLine.Product).State = System.Data.Entity.EntityState.Unchanged;
                }



                var _order = db.OrderMaster.FirstOrDefault(o => o.Id == order.Id);
                _order.OrderTotal = divedSum;
                db.Entry(_order).State = System.Data.Entity.EntityState.Modified;
                db.Entry(_order.Outlet).State = System.Data.Entity.EntityState.Unchanged;
            }
            db.SaveChanges();
            return true;
        }

        public bool SplitOrderLine(OrderLine selectedItem, int number, string userId)
        {
            decimal discount = 0;

            var line = db.OrderDetail.FirstOrDefault(o => o.Id == selectedItem.Id);
            db.OrderDetail.Remove(line);

            var qty = selectedItem.Quantity / number;

            if (selectedItem.ItemDiscount > 0)
                discount = selectedItem.ItemDiscount / number;

            for (int i = 1; i <= number; i++)
            {
                var newline = selectedItem.GetFrom();
                newline.Id = Guid.NewGuid();
                newline.Quantity = qty;
                newline.ItemDiscount = discount;

                db.OrderDetail.Add(newline);
                //context.Entry(newline.Product).State = System.Data.Entity.EntityState.Unchanged;

                //Spliting of Ingredients
                var ingredients = db.OrderDetail.Where(o => o.GroupId == selectedItem.ItemId && o.OrderId == selectedItem.OrderId).ToList();
                db.OrderDetail.RemoveRange(ingredients);
                //new code for log
                var order = db.OrderMaster.FirstOrDefault(o => o.Id == newline.OrderId);
                Guid newOrderID = Guid.Empty;
                if (order != null)
                    newOrderID = order.TerminalId;
                //
                foreach (var newinnerLine in ingredients)
                {
                    var newRec = selectedItem.GetFrom();
                    newRec.Id = Guid.NewGuid();
                    newRec.ItemId = newinnerLine.ItemId;
                    newRec.Quantity = newinnerLine.Quantity / number;
                    newRec.UnitPrice = newinnerLine.UnitPrice;
                    newRec.DiscountedUnitPrice = newinnerLine.DiscountedUnitPrice;
                    newRec.GroupId = newline.ItemId;
                    newRec.ItemType = ItemType.Ingredient;
                    newRec.IngredientMode = "+";
                    newRec.GroupKey = newline.Id;
                    newRec.ItemDiscount = 0;


                    db.OrderDetail.Add(newRec);

                    /*Ingredients in journal*/
                    var journalIngredient = new Journal
                    {
                        OrderId = newline.OrderId,
                        ItemId = newinnerLine.ItemId,
                        ActionId = Convert.ToInt32(JournalActionCode.ItemAdded),
                        Created = DateTime.Now,
                        UserId = userId,
                        LogMessage = "Ingredients " + "Order# " + newline.OrderId,
                        TerminalId = newOrderID

                    };
                    db.Journal.Add(journalIngredient);
                }

                /*Item in journal*/
                var journal = new Journal
                {
                    OrderId = newline.OrderId,
                    ItemId = newline.ItemId,
                    ActionId = Convert.ToInt32(JournalActionCode.ItemAdded),
                    Created = DateTime.Now,
                    UserId = userId,
                    TerminalId = newOrderID,

                };
                db.Journal.Add(journal);

            }



            db.SaveChanges();

            return true;
        }

        public Order UpdateSpliteOrder(Order orderViewModel, List<OrderLine> oldOrderDetails, decimal OldOrderTotal)
        {
            using (var db = new ApplicationDbContext())
            {

                var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderViewModel.Id);
                if (oldOrderDetails.Count == 0)
                {
                    db.OrderMaster.Remove(order);
                    //db.Entry(order.Outlet).State = System.Data.Entity.EntityState.Unchanged;
                }
                else
                {
                    order.OrderTotal = OldOrderTotal;

                    var orderLines = db.OrderDetail.Where(o => o.OrderId == order.Id).ToList();
                    db.OrderDetail.RemoveRange(orderLines);
                    //  db.OrderDetail.AddRange(oldOrderDetails);

                    foreach (var orderLine in oldOrderDetails)
                    {

                        orderLine.Id = Guid.NewGuid();
                        orderLine.OrderId = order.Id;
                        db.OrderDetail.Add(orderLine);
                        db.Entry(orderLine.Product).State = System.Data.Entity.EntityState.Detached;


                        //   Adding Ingredient
                        if (orderLine.IngredientItems != null)
                        {
                            foreach (var _orderLine in orderLine.IngredientItems)
                            {
                                _orderLine.Id = Guid.NewGuid();
                                _orderLine.OrderId = order.Id;
                                _orderLine.ItemId = _orderLine.ItemId;
                                _orderLine.GroupId = orderLine.ItemId;
                                _orderLine.GroupKey = orderLine.Id;

                                db.OrderDetail.Add(_orderLine);
                                db.Entry(_orderLine.Product).State = System.Data.Entity.EntityState.Detached;

                            }
                        }
                        //   Add detail if group item
                        if (orderLine.ItemDetails != null)
                        {
                            foreach (var _orderLine in orderLine.ItemDetails)
                            {

                                _orderLine.Id = Guid.NewGuid();
                                _orderLine.OrderId = order.Id;
                                db.OrderDetail.Add(_orderLine);
                                db.Entry(_orderLine.Product).State = System.Data.Entity.EntityState.Detached;

                            }
                        }
                    }
                }
                db.SaveChanges();

                return order;
            }


        }

        public List<OrderLine> CancelOrderDetail(Guid orderId, Guid oldOrderId, string userId)
        {

            List<OrderLine> orderDetail = db.OrderDetail.Where(o => o.OrderId == orderId).ToList();
            var order = db.OrderMaster.FirstOrDefault(o => o.Id == oldOrderId);

            foreach (var orderLine in orderDetail)
            {
                var line = db.OrderDetail.FirstOrDefault(o => o.OrderId == oldOrderId && o.Product == orderLine.Product && o.UnitPrice == orderLine.UnitPrice);
                if (line != null)
                {
                    line.Quantity = line.Quantity + orderLine.Quantity;
                    line.OrderId = order.Id;
                }
                else
                {
                    if (order != null)
                    {
                        var newLine = new OrderLine
                        {
                            Id = Guid.NewGuid(),
                            ItemId = orderLine.ItemId,
                            UnitPrice = orderLine.UnitPrice,
                            Quantity = orderLine.Quantity,
                            Direction = orderLine.Direction,
                            DiscountedUnitPrice = orderLine.DiscountedUnitPrice,
                            DiscountPercentage = orderLine.DiscountPercentage,
                            ItemDiscount = orderLine.ItemDiscount,
                            Active = orderLine.Active,
                            IsCoupon = orderLine.IsCoupon,
                            ItemComments = orderLine.ItemComments,
                            ItemStatus = orderLine.ItemStatus,
                            OrderId = order.Id,
                            PurchasePrice = orderLine.PurchasePrice,
                            TaxPercent = orderLine.TaxPercent,
                            UnitsInPackage = orderLine.UnitsInPackage
                        };
                        db.OrderDetail.Add(newLine);
                    }
                    //db.Entry(newLine.Product).State = System.Data.Entity.EntityState.Unchanged;
                }
            }

            var removeOrder = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
            removeOrder.Status = OrderStatus.OrderCancelled;
            db.Entry(removeOrder).State = System.Data.Entity.EntityState.Modified;
            var journal = new Journal
            {
                ActionId = Convert.ToInt16(JournalActionCode.OrderCancelled),
                Created = DateTime.Now,
                OrderId = removeOrder.Id,
                UserId = userId,
                TerminalId = order.TerminalId
            };
            db.Journal.Add(journal);
            var orderDetails = db.OrderDetail.Where(o => o.Id == oldOrderId).ToList();
            order.OrderTotal = orderDetails.Sum(s => s.GrossAmountDiscounted());
            order.Status = OrderStatus.AssignedKitchenBar;
            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
            if (order.Outlet != null)
                db.Entry(order.Outlet).State = System.Data.Entity.EntityState.Unchanged;

            db.SaveChanges();

            return GetOrderLinesById(oldOrderId);
        }


        public bool UpdateOrderDetail(List<OrderLine> orderDetail, Order order)
        {
            foreach (var orderLineModel in orderDetail)
            {
                var line = db.OrderDetail.Single(ol => ol.Id == orderLineModel.Id);
                line.OrderId = order.Id;
                db.Entry(line).State = System.Data.Entity.EntityState.Modified;
                db.Entry(line.Product).State = System.Data.Entity.EntityState.Unchanged;
            }
            db.SaveChanges();

            UpdateResetOrderTotal(order.Id);
            return true;
        }
        public bool UpdateResetOrderTotal(Guid orderId)
        {
            decimal orderTotal = 0;
            var orderLines = db.OrderDetail.Where(o => o.OrderId == orderId).ToList();
            if (orderLines != null && orderLines.Count > 0)
                orderTotal = orderLines.Sum(s => s.GrossAmountDiscounted());
            var order = db.OrderMaster.FirstOrDefault(o => o.Id == orderId);
            order.OrderTotal = orderTotal;
            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
            if (order.Outlet != null)
                db.Entry(order.Outlet).State = System.Data.Entity.EntityState.Unchanged;
            db.SaveChanges();
            return true;
        }
        public List<Order> MoveOrders(List<Order> selectedOrders, FoodTable selectedTable, OrderType type, string comments)
        {

            var previouseTableId = 0;

            foreach (var currentOrder in selectedOrders)
            {
                previouseTableId = currentOrder.TableId;
                var order = db.OrderMaster.FirstOrDefault(o => o.Id == currentOrder.Id);
                if (order != null)
                {
                    order.Type = type;
                    if (type == OrderType.TakeAway)
                    {
                        order.TableId = 0;
                        order.Comments = comments;
                    }
                    else
                    {
                        order.TableId = selectedTable.Id;
                        order.Comments = selectedTable.Name;
                    }
                    db.Entry(order).State = System.Data.Entity.EntityState.Modified;
                    //db.Entry(order.Outlet).State = System.Data.Entity.EntityState.Unchanged;
                }
            }
            db.SaveChanges();

            return selectedOrders;

        }

        public Order MergeOrders(List<Order> selectedOrders, FoodTable selectedTable, OrderType type, string userId, Guid terminalId, Guid outletId, int shiftNo)
        {

            Guid orderId = default(Guid);

            var orderLines = new List<OrderLine>();
            var itemForLogs = new List<Guid>();

            db.Configuration.LazyLoadingEnabled = false;
            foreach (var currentOrder in selectedOrders)
            {
                var lines = db.OrderDetail.Include("Product").Where(o => o.OrderId == currentOrder.Id && o.Active == 1).ToList();
                if (lines.Count > 0)
                {
                    orderLines.AddRange(lines);
                }

            }
            if (orderLines.Count > 0)
            {
                var groups = orderLines.GroupBy(g => new { g.ItemId, g.UnitPrice, g.GroupId }).ToList();
                var newLines = new List<OrderLine>();
                foreach (var grp in groups)
                {
                    var line = grp.First();
                    line.Quantity = grp.Sum(s => s.Quantity);
                    newLines.Add(line);
                }
                var orderTotal =
                    newLines.Select(s => new { GrossTotal = s.UnitPrice * s.Direction * s.Quantity })
                        .Sum(ol => ol.GrossTotal);

                int lastNo = 0;
                var ordDate = DateTime.Now.Date;
                var ord =
                    db.OrderMaster.OrderByDescending(o => o.CreationDate)
                        .FirstOrDefault(o => o.OrderNoOfDay != "" && o.CreationDate >= ordDate);
                string[] orNo = ord?.OrderNoOfDay.Split('-');

                if (orNo?.Length > 1)
                    int.TryParse(orNo[1], out lastNo);

                string orderNoOfDay = DateTime.Now.Year + DateTime.Now.Month.ToString() + DateTime.Now.Day + "-" +
                                      (lastNo + 1);


                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    TableId = selectedTable.Id,
                    CreationDate = DateTime.Now,
                    OrderTotal = orderTotal,
                    UserId = userId,
                    Status = OrderStatus.AssignedKitchenBar,
                    ShiftNo = shiftNo,
                    Updated = 1,
                    PaymentStatus = 2,
                    Comments = selectedTable.Name,
                    Type = type,
                    TerminalId = terminalId,
                    OutletId = outletId,
                    OrderNoOfDay = orderNoOfDay
                };

                db.OrderMaster.Add(order);

                foreach (var line in newLines)
                {
                    itemForLogs.Add(line.ItemId);
                    line.OrderId = order.Id;
                    db.Entry(line).State = System.Data.Entity.EntityState.Modified;
                }
                string[] ary = new string[selectedOrders.Count];

                int i = 0;
                foreach (var currentOrder in selectedOrders)
                {
                    ary[i] = currentOrder.Id.ToString();

                    i++;
                }
                foreach (string id in ary)
                {
                    Guid ordId = Guid.Parse(id);
                    var item = db.OrderMaster.FirstOrDefault(o => o.Id == ordId);
                    db.OrderMaster.Remove(item);

                }
                db.SaveChanges();
                orderId = order.Id;

            }

            return GetOrderMasterDetailById(orderId);

        }

        #region Order Hisotry
        public List<OrderLine> GetSaleHistory(string queryText, DateTime startDate, DateTime endDate)
        {
            string dt1 = startDate.Year + "-" + startDate.Month + "-" + startDate.Day;
            string dt2 = endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " 11:59:00 PM";

            List<OrderLine> orders = new List<OrderLine>();

            string query = @"SELECT  [dbo].[Fn_CategoryByItem]( OrderDetail.ItemId) as Category,Product.Description as ItemName,Sum(OrderDetail.Qty) as Qty,OrderDetail.UnitPrice , Sum((OrderDetail.UnitPrice*OrderDetail.Qty*OrderDetail.Direction)-OrderDetail.ItemDiscount) as Total
	                   FROM OrderMaster LEFT JOIN OrderDetail 
			        ON OrderMaster.Id = OrderDetail.OrderId 
			        inner join Product on OrderDetail.ItemId=Product.Id
				        WHERE   OrderMaster.TrainingMode=0 AND  OrderMaster.InvoiceDate  BETWEEN '" + dt1 + "' AND '" + dt2 + "' AND OrderMaster.InvoiceGenerated=1 AND OrderMaster.Status<>14   AND OrderDetail.ItemType<>1  AND OrderDetail.Active=1" +
                "GROUP BY [dbo].[Fn_CategoryByItem]( OrderDetail.ItemId),Product.Description,OrderDetail.UnitPrice";

            IDbCommand command = new SqlCommand();
            db.Database.Connection.Open();
            command.Connection = db.Database.Connection;

            command.CommandText = query;



            IDataReader dr = command.ExecuteReader();
            while (dr.Read())
            {
                orders.Add(new OrderLine
                {
                    Category = Convert.ToString(dr["Category"]),
                    Description = Convert.ToString(dr["ItemName"]),
                    Quantity = Convert.ToDecimal(dr["Qty"]),
                    UnitPrice = Convert.ToDecimal(dr["UnitPrice"]),
                    TaxPercent = Convert.ToDecimal(dr["Total"]),


                });
            }
            dr.Dispose();

            return orders;
        }
        public List<Order> SearchGeneralOrder(string queryText, DateTime startDate, DateTime endDate)
        {


            string dt1 = startDate.Year + "-" + startDate.Month + "-" + startDate.Day;
            string dt2 = endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " 11:59:00 PM";
            List<Order> orders = new List<Order>();

            string query = @"SELECT OrderMaster.Status, OrderMaster.Id, Receipt.ReceiptNumber, OrderMaster.OrderNoOfDay,OrderMaster.OrderComments, OrderMaster.InvoiceDate, OrderMaster.OrderTotal, OrderMaster.InvoiceGenerated
                         FROM  OrderMaster INNER JOIN
                              Receipt ON OrderMaster.Id = Receipt.OrderId 
                                left join Customer on Customer.Id = OrderMaster.CustomerId
            Where OrderMaster.Status not in (17,18) and OrderMaster.InvoiceGenerated=1 AND OrderMaster.InvoiceGenerated=1 ";



            if (!string.IsNullOrEmpty(queryText))
            {
                query = query + " AND OrderMaster.OrderNoOfDay like '%" + queryText + "%'";
            }
            else
            {
                query = query + " AND InvoiceDate between '" + dt1 + "' AND '" + dt2 + "'";
            }

            using (var conn = db.Database.Connection)
            {
                IDbCommand command = new SqlCommand();
                conn.Open();
                command.Connection = conn;

                command.CommandText = query;


                IDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    orders.Add(new Order
                    {
                        Id = Guid.Parse(dr["Id"].ToString()),
                        InvoiceNumber = Convert.ToString(dr["ReceiptNumber"]),
                        OrderComments = Convert.ToString(dr["OrderComments"]),
                        OrderNoOfDay = Convert.ToString(dr["OrderNoOfDay"]),
                        InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                        OrderTotal = Convert.ToDecimal(dr["OrderTotal"]),
                        Status = (OrderStatus)(Convert.ToInt32(dr["Status"])),
                        InvoiceGenerated = Convert.ToInt32(dr["InvoiceGenerated"])
                    });
                }
                dr.Dispose();

            }
            return orders;
        }

        public List<Order> SearchGeneralOrderCCFailed(string queryText, DateTime startDate, DateTime endDate)
        {


            string dt1 = startDate.Year + "-" + startDate.Month + "-" + startDate.Day;
            string dt2 = endDate.Year + "-" + endDate.Month + "-" + endDate.Day + " 11:59:00 PM";
            List<Order> orders = new List<Order>();

            string query = @"SELECT OrderMaster.Status, OrderMaster.Id, Receipt.ReceiptNumber, OrderMaster.OrderNoOfDay,OrderMaster.OrderComments, OrderMaster.InvoiceDate, OrderMaster.OrderTotal, OrderMaster.InvoiceGenerated
                    FROM  OrderMaster INNER JOIN
                         Receipt ON OrderMaster.Id = Receipt.OrderId 
						 Where OrderMaster.Status in (17,18) and OrderMaster.InvoiceGenerated=1 AND OrderMaster.InvoiceGenerated=1 ";
            if (!string.IsNullOrEmpty(queryText))
            {
                query = query + " AND OrderMaster.OrderNoOfDay like '%" + queryText + "%'";
            }
            else
            {
                query = query + " AND InvoiceDate between '" + dt1 + "' AND '" + dt2 + "'";
            }

            using (var conn = db.Database.Connection)
            {
                IDbCommand command = new SqlCommand();
                conn.Open();
                command.Connection = conn;

                command.CommandText = query;


                IDataReader dr = command.ExecuteReader();
                while (dr.Read())
                {
                    orders.Add(new Order
                    {
                        Id = Guid.Parse(dr["Id"].ToString()),
                        InvoiceNumber = Convert.ToString(dr["ReceiptNumber"]),
                        OrderComments = Convert.ToString(dr["OrderComments"]),
                        OrderNoOfDay = Convert.ToString(dr["OrderNoOfDay"]),
                        InvoiceDate = Convert.ToDateTime(dr["InvoiceDate"]),
                        OrderTotal = Math.Round(Convert.ToDecimal(dr["OrderTotal"]), 2),
                        Status = (OrderStatus)(Convert.ToInt32(dr["Status"])),
                        InvoiceGenerated = Convert.ToInt32(dr["InvoiceGenerated"])
                    });
                }
                dr.Dispose();

            }
            return orders;
        }

        #endregion



        #region PosMIniOrder Chekout



        #endregion












        public void Dispose()
        {
            db.Dispose();
        }

        public List<OrderLine> GetOrderLinesByOrder(Guid orderId)
        {
            return db.OrderDetail.Where(o => o.OrderId == orderId).ToList();
        }
    }
}