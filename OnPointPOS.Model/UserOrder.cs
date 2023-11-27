using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class UserOrder : BaseEntity
    {
        public UserOrder()
        {

        }

        [Key]
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        public Guid OrderBy { get; set; }
        public string DeviceInformation { get; set; }

        #region Not Mapped Properties

        public UserOrder(UserOrder userOrder)
        {
            Id = userOrder.Id;
            OrderId = userOrder.OrderId;
            OrderBy = userOrder.OrderBy;
            DeviceInformation = userOrder.DeviceInformation;
        }
        
        #endregion
    }
}
