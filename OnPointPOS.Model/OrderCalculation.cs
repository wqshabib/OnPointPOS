using System;
using System.Collections.Generic;

namespace POSSUM.Model
{
    public partial class Order
    {
        private decimal ItemDiscount(decimal price, decimal quantity, decimal percent)
        {
            decimal grossTotal = quantity * price;

            decimal itemdiscount = grossTotal / 100 * 5;
            itemdiscount = quantity * itemdiscount;

            return itemdiscount;
        }

        private decimal ItemCampaign(Campaign campaign)
        {
            return 0;
        }
    }
}