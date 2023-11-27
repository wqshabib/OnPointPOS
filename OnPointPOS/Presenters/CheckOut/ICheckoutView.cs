using System;
using System.Collections.Generic;
using POSSUM.Base;
using POSSUM.Model;

namespace POSSUM.Presenters.CheckOut
{
    public interface ICheckoutView : IBaseView
    {
        Guid GetOrderId();

        void SetParameters(decimal totalOrderAmount, Guid orderId, int _direction, List<OrderLine> orderDetail,
            int tableId);
        void SetVatAmount(decimal p);
    }
}