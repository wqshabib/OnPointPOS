using System;
using System.Collections.Generic;
using POSSUM.Base;
using POSSUM.Model;

namespace POSSUM.Presenters.OrderHistory
{
    public interface IOrderHistoryView : IBaseView
    {
        DateTime GetStartdate();

        DateTime GetEnddate();

        DateTime GetStartdateCCFailed();

        DateTime GetEnddateCCFailed();

        string GetQueryText();
        string GetQueryTextCCFailed();

        void SetTotalAmount(decimal total);
        void SetTotalAmountCCFailed(decimal total);

        void SetResult(List<Order> orders);
        void SetResultCCFailed(List<Order> orders);
    }
}