using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.MasterData
{
    public class Company
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string Address { get; set; }
        [MaxLength(50)]
        public string City { get; set; }
        [MaxLength(10)]
        public string PostalCode { get; set; }
        [MaxLength(50)]
        public string Country { get; set; }
        [MaxLength(150)]
        public string AdminURL { get; set; }
        public bool Active { get; set; }
        [MaxLength(150)]
        public string DBServer { get; set; }
        [MaxLength(50)]
        public string DBName { get; set; }
        [MaxLength(50)]
        public string DBUser { get; set; }
        [MaxLength(50)]
        public string DBPassword { get; set; }
        public Company()
        {
            Id = default(Guid);
            Active = true;
        }
        [NotMapped]
        public string ConnectionString
        {
            get
            {
                return @"Data Source=" + DBServer + ";Initial Catalog=" + DBName + ";User Id=" + DBUser + ";Password=" + DBPassword;
            }
        }

        [NotMapped]
        public string UserEmail { get; set; }
        [NotMapped]
        public string UserName { get; set; }
        [NotMapped]
        public string UserPassword { get; set; }
    }
    public class Contact
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [MaxLength(50)]
        public string Username { get; set; }
        [MaxLength(50)]
        public string Password { get; set; }
        [MaxLength(150)]
        public string Email { get; set; }
        [MaxLength(50)]
        public string Phone { get; set; }
        [MaxLength(500)]
        public string Notes { get; set; }
        public Contact()
        {
            Id = Id = default(Guid);
        }
    }
    public class Contract
    {
        [Key]
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        public ContractStatus Status { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MonthlyPrice { get; set; }
        public DateTime Expected_Deployment_Date { get; set; }
        public DateTime Actual_Deployment_Date { get; set; }
        [MaxLength(500)]
        public string Notes { get; set; }
        [MaxLength(50)]
        public string POSVersion { get; set; }
        public Contract()
        {
            Id = default(Guid);
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            Expected_Deployment_Date = DateTime.Now;
            Actual_Deployment_Date = DateTime.Now;
        }
    }
    public enum ContractStatus
    {
        Draft = 0, Ready = 1
    }
    public class SeamlessClient
    {
        [Key]
        public int Id { get; set; }
        public Guid CompanyId { get; set; }
        public string ClientId { get; set; }
        public string Password { get; set; }
        public long ClientRequestTimeout { get; set; }
        public string Web_url { get; set; }

    }
}
