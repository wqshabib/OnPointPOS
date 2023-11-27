using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    //[Serializable]
    public class Cart : Result
    {
        public int TotalQuantity { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal CustomerPaymentFee { get; set; }

        public List<CartItem> CartItems = new List<CartItem>();
        //public List<CartItem> CartItems { get; set; }
    }

    public class CartItem// : List<CartSubItem>
    {
        public List<CartSubItem> CartSubItems = new List<CartSubItem>();
        //public List<CartSubItem> CartSubItems { get; set; }

        public string CartId { get; set; }
        public string Title { get; set; }
        public string ContentVariant { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }

        //public int Quantity { get; set; }
    }

    public class CartSubItem
    {
        public string Text { get; set; }
        public decimal Price { get; set; }
        public decimal VAT { get; set; }

        //public int Quantity { get; set; }
    }



}