using System;
using System.Collections.Generic;

namespace POSSUM.Model
{
    public class UserData
    {
        public List<User> Users { get; set; }
        public List<Role> Roles { get; set; }
        public List<OutletUser> TillUsers { get; set; }
        public List<UserRole> UserRoles { get; set; }
    }

    public class UserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
		public string Password { get; set; }
        public string DallasKey { get; set; }
        public Guid TerminalId { get; set; }
        public Guid OutletId { get; set; }
    }
}