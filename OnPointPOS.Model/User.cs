using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace POSSUM.Model
{
    public class AppUser
    {
        public virtual string applicationuser_key { get; set; }
    }
    public class User:BaseEntity
    {
        public virtual string Id { get; set; }
        public virtual string Email { get; set; }
        public virtual string UserName { get; set; }        
        public virtual string Password { get; set; }
        public virtual bool TrainingMode { get; set; }
        public virtual DateTime LockoutEndDateUtc { get; set; }
        public virtual bool LockoutEnabled { get; set; }
        public virtual bool EmailConfirmed { get; set; }
        public virtual string PasswordHash { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual bool PhoneNumberConfirmed { get; set; }
        public virtual string SecurityStamp { get; set; }
        public virtual bool TwoFactorEnabled { get; set; }
        public virtual int AccessFailedCount { get; set; }
        public virtual bool Active { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Updated { get; set; }

        public virtual string DallasKey { get; set; }

    }
    public class Role
    {
        public virtual string Id { get; set; }
        public virtual string Name { get; set; }
    }
    public class UserRole
    {
        public virtual string UserId { get; set; }
        public virtual string RoleId { get; set; }
        public virtual string RoleName { get; set; }
        public virtual string UserName { get; set; }
    }
  
}
