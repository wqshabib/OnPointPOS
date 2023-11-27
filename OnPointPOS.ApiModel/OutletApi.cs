using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.ApiModel
{
    public class OutletApi
    {
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
        public List<TerminalApi> Terminals { get; set; }

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

        public static OutletApi ConvertModelToApiModel(Outlet outlet)
        {
            return new OutletApi
            {
                Id = outlet.Id,
                Name = outlet.Name,
                Address1 = outlet.Address1,
                Address2 = outlet.Address2,
                Address3 = outlet.Address3,
                City = outlet.City,
                PostalCode = outlet.PostalCode,
                BillPrinterId = outlet.BillPrinterId,
                KitchenPrinterId = outlet.KitchenPrinterId,
                IsDeleted = outlet.IsDeleted,
                Email = outlet.Email,
                WebUrl = outlet.WebUrl,
                Phone = outlet.Phone,
                OrgNo = outlet.OrgNo,
                HeaderText = outlet.HeaderText,
                FooterText = outlet.FooterText,
                TaxDescription = outlet.TaxDescription,
                Created = outlet.Created,
                Updated = outlet.Updated,
                WarehouseID = outlet.WarehouseID,
                UniqueCode = outlet.UniqueCode,
                Active = outlet.Active
            };
        }
    }
}