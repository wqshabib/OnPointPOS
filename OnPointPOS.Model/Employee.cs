using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    public class Employee : BaseEntity
    {
        public Employee()
        {
            Created = DateTime.Now;
        }

        [Key]
        public virtual Guid Id { get; set; }
        [MaxLength(50)]
        public virtual string FirstName { get; set; }
        [MaxLength(50)]
        public virtual string LastName { get; set; }
        [MaxLength(50)]
        public virtual string SSNO { get; set; }
        [MaxLength(150)]
        public virtual string Email { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Updated { get; set; }

        [NotMapped]
        public virtual IList<EmployeeLog> Logs { get; set; }

        [NotMapped]
        public virtual string Name
        {
            get
            {
                return (this.FirstName + " " + this.LastName);
            }
            set { }
        }
    }

    public class EmployeeLog : BaseEntity
    {
        [Key]
        public virtual Guid LogId { get; set; }

        public virtual Guid EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
       
        public virtual Employee Employee { get; set; }

        public virtual DateTime? CheckIn { get; set; }
        public virtual DateTime? CheckOut { get; set; }
        public virtual bool Completed { get; set; }
        public virtual int Synced { get; set; }
        [NotMapped]
        public string EmployeeName { get; set; }
        [NotMapped]
        public string SSNo { get; set; }
    }
}