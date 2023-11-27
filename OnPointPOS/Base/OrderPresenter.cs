using System.Collections.Generic;
using POSSUM.Model;
using System;
namespace POSSUM.Base
{
    public abstract class OrderPresenter : IOrderPrsenter
    {
        protected OrderPresenter()
        {
        }

        protected OrderPresenter(Order masterOrder)
        {
            MasterOrder = masterOrder;
        }

        public List<OrderLine> CurrentorderDetails { get; set; }
        //public List<OrderLine> CurrentorderDetails { get; set; }
        public Dictionary<int, decimal> CampaignDictionary { get; set; }
        public Dictionary<int, List<OrderLine>> CampaignList { get; set; }
        public Order MasterOrder { get; set; }

        //NEED TO MERGE BELOW ITEMS WITH MASTER ORDER
        public OrderType Type { get; set; }

        public FoodTable SelectedTable { get; set; }
    }
}