using POSSUM.Data;
using POSSUM.Model;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils.Controller
{
    public class UserController
    {
        List<User> liveUsers = new List<User>();
        List<Role> liveRoles = new List<Role>();
        List<UserRole> liveUserRoles = new List<UserRole>();
        List<OutletUser> liveTillUsers = new List<OutletUser>();
        string connectionString;
        public UserController(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public UserController(List<User> liveUsers, List<Role> liveRoles, List<OutletUser> liveTillUsers, List<UserRole> liveUserRoles, string connectionString)
        {
            this.liveRoles = liveRoles;
            this.liveUsers = liveUsers;
            this.liveTillUsers = liveTillUsers;
            this.liveUserRoles = liveUserRoles;
			this.connectionString = connectionString;

		}
        public bool UpdateUser()
        {
            try
            {

				if (liveTillUsers != null && liveTillUsers.Count > 0)
				{

					using (var uofLocal = new UnitOfWork(new ApplicationDbContext(connectionString)))
					{

						var localTillUsersRepo = uofLocal.TillUserRepository;


						foreach (var tillUser in liveTillUsers)
						{

							localTillUsersRepo.AddOrUpdate(tillUser);

						}


						uofLocal.Commit();
					}


				}
				return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex.ToString());
                return false;
            }
          
        }

    }
}
