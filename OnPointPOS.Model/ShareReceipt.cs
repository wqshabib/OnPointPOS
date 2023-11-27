using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class ShareReceipt
    {
        public ShareReceipt()
        {

        }

        public string ShareWith;
        public string ShareMode;
        public Order Order { get; set; }
    }
}