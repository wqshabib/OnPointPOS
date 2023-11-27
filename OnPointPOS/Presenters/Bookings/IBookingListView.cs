using POSSUM.Base;
using POSSUM.Data.Repositories;
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Presenters.Bookings
{
    public interface IBookingListView : IBaseView
    {
        DateTime GetStartdate();

        DateTime GetEnddate();

        DateTime GetStartdateCCFailed();

        DateTime GetEnddateCCFailed();

        string GetQueryText();
        string GetQueryTextCCFailed();

        void SetTotalAmount(decimal total);
        void SetTotalAmountCCFailed(decimal total);

        void SetResult(List<BookingViewModel> bookings); 
        void SetResultCCFailed(List<BookingViewModel> bookings);
    }
}
