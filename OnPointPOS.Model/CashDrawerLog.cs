using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public enum DrawerActivityType
    {
        OpeningBalance = 1,
        PaidToEmployee = 2,
        PaidBill = 3,
        CashDrop = 4,
        CashAdded = 5,
        CashBySale = 6,
        CashByTip = 7,
        PaidTipToEmployee = 8,
        Banked = 9,
        Float = 10,
        Close = 11
    }

    [Serializable]
    public class CashDrawerLog : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CashDrawerId { get; set; }

        [ForeignKey("CashDrawerId")]
        public virtual CashDrawer CashDrawer { get; set; }

        public DateTime ActivityDate { get; set; }
        public DrawerActivityType ActivityType { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public Guid OrderId { get; set; }
        public int Synced { get; set; }
    }
}