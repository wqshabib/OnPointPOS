using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class SwishPayment : BaseEntity
    {
        public SwishPayment()
        {

        }

        [Key]
        public Guid Id { get; set; }
        public string SwishId { get; set; }
        public string SwishPaymentId { get; set; }
        public int SwishPaymentStatus { get; set; }
        public string SwishResponse { get; set; }
        public string SwishLocation { get; set; }
        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        #region Not Mapped Properties

        public SwishPayment(SwishPayment swishPayment)
        {
            Id = swishPayment.Id;
            SwishId = swishPayment.SwishId;
            SwishPaymentId = swishPayment.SwishPaymentId;
            SwishPaymentStatus = swishPayment.SwishPaymentStatus;
            SwishResponse = swishPayment.SwishResponse;
            SwishLocation = swishPayment.SwishLocation;
            OrderId = swishPayment.OrderId;
        }
        
        #endregion
    }
}
