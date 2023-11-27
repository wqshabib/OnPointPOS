using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Customer : BaseEntity
    {
        public Customer()
        {
            Created = DateTime.Now;
            Active = true;
        }

        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(150)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string OrgNo { get; set; }
        [MaxLength(250)]
        public string Address1 { get; set; }
        [MaxLength(250)]
        public string Address2 { get; set; }
        public int FloorNo { get; set; }
        public int PortCode { get; set; }
        public string CustomerNo { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(50)]
        public string Phone { get; set; }
        [MaxLength(50)]
        public string ZipCode { get; set; }
        [MaxLength(50)]
        public string Reference { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public bool DirectPrint { get; set; }
        public bool Active { get; set; }
        public string PinCode { get; set; }

        //New Field for Visma integration- khalil
        public string ExternalId { get; set; }
        public string Email { get; set; }
        public DateTime? LastBalanceUpdated { get; set; }
        public decimal DepositAmount { get; set; }
        public bool HasDeposit { get; set; } 

        public List<Customer_CustomField> Customer_CustomField { get; set; }
    }
    public class Customer_CustomField : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FieldId { get; set; }
        //[ForeignKey("FieldId")]
        //public CustomerCustomField CustomerCustomField { get; set; }
        public Guid CustomerId { get; set; }
        //[ForeignKey("CustomerId")]
        //public Customer Customer { get; set; }
        public string Caption { get; set; }
        public string Text { get; set; }
        public int SortOrder { get; set; }
        public DateTime Updated { get; set; }
    }
    public class CustomerCustomField
    {
        [Key]
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public int SortOrder { get; set; }
    }
    public class CustomerBonus : BaseEntity
    {

        [Key]
        public virtual Guid Id { get; set; }
        public virtual Guid CustomerId { get; set; }
        public virtual Guid OrderId { get; set; }
        public virtual Guid OutletId { get; set; }
        public virtual decimal ChangeValue { get; set; }
        public virtual decimal CurrentSum { get; set; }
        public virtual DateTime CreatedOn { get; set; }

    }
}