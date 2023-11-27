using POSSUM.Model;
using System;

namespace POSSUM.ApiModel
{
    public class DatesApi
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public Guid OutletId { get; set; }
        public Guid TerminalId { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
    }
}
