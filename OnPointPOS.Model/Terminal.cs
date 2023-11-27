using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POSSUM.Model
{
    [Serializable]
    public class Terminal:BaseEntity
    {
        public enum TerminalStatus
        {
            CLOSED = 0,
            OPEN = 1
        }
        [Key]
        public virtual Guid Id { get; set; }
        public virtual Guid OutletId { get; set; }
        [ForeignKey("OutletId")]
        public virtual  Outlet Outlet { get; set; }
        public virtual int TerminalNo { get; set; }
        public virtual Guid TerminalType { get; set; }
        public virtual string UniqueIdentification { get; set; }
        public virtual string HardwareAddress { get; set; }
        public virtual string Description { get; set; }
        [NotMapped]
        public virtual IList<TerminalStatusLog> StatusLog { get; set; }
        public virtual TerminalStatus Status { get; set; }
        [NotMapped]
        public virtual IList<CashDrawer> CashDrawer { get; set; }
        public virtual int RootCategoryId { get; set; }
        [ForeignKey("RootCategoryId")]
        public virtual Category Category { get; set; }   
        public virtual bool IsDeleted { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Updated { get; set; }
        public virtual string CCUData { get; set; }
        public virtual bool AutoLogin { get; set; }

        /// <summary>
        /// Set Terminal status
        /// </summary>
        /// <param name="userId">User id changing terminal status</param>
        public virtual void Open(string userId, decimal openingAmount)
        {
            if (IsOpen)
            {
                // throw new ArgumentException("Terminal already open");
                return;
            }
            if (StatusLog == null)
                StatusLog = new List<TerminalStatusLog>();
            Status = TerminalStatus.OPEN;
            StatusLog.Add(

                new TerminalStatusLog()
                {
                    Id = Guid.NewGuid(),
                    ActivityDate = DateTime.Now,
                    UserId = userId,
                    Status = TerminalStatus.OPEN,
                    Terminal = this
                }
                );

        }

        public virtual void Close(string userId)
        {
            if (!IsOpen)
            {
                throw new ArgumentException("Terminal already closed");

            }
            if (StatusLog == null)
                StatusLog = new List<TerminalStatusLog>();
            Status = TerminalStatus.CLOSED;
            StatusLog.Add(

                new TerminalStatusLog()
                {
                    Id = Guid.NewGuid(),
                    ActivityDate = DateTime.Now,
                    UserId = userId,
                    Status = TerminalStatus.CLOSED,
                    Terminal = this
                }
                );

        }
        [NotMapped]
        public virtual bool IsOpen { get { return Status == TerminalStatus.OPEN; } }

    }
}
