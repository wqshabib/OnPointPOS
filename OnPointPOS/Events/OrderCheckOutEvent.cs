using POSSUM.Model;
using System;
using System.Collections.Generic;

namespace POSSUM.Events
{
    public delegate void OrderCheckOutEventHandler(object sender, Order orderMaster,List<OrderLine> orderDetail);
    public delegate void OrderDirectCheckOutEventHandler(object sender,Guid orderId,bool isCreditCard);

}
