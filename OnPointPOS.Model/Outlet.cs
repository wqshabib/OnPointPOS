using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    [Serializable]
    public class Outlet : BaseEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public int BillPrinterId { get; set; }
        public int KitchenPrinterId { get; set; }
        public bool IsDeleted { get; set; }

        public string Email { get; set; }
        public string WebUrl { get; set; }
        public string Phone { get; set; }
        public string OrgNo { get; set; }
        public string HeaderText { get; set; }
        public string FooterText { get; set; }
        public string TaxDescription { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
		public Guid WarehouseID { get; set; }
        public string UniqueCode { get; set; }
        public bool Active { get; set; }
        [NotMapped]
        public bool HasAddress => !string.IsNullOrEmpty(Address);

        [NotMapped]
        public string Address
        {
            get
            {
                var address = string.Empty;
                if (!string.IsNullOrEmpty(Address1))
                    address = Address1;
                if (!string.IsNullOrEmpty(PostalCode))
                    address += "\n" + PostalCode;
                if (!string.IsNullOrEmpty(City))
                    address += ", " + City;
                return address;
            }
        }
    }
}