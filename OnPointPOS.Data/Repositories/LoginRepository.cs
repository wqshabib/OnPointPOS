
using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class LoginRepository : GenericRepository<OutletUser>, IDisposable
    {
        private readonly ApplicationDbContext context;

        public LoginRepository(ApplicationDbContext context) : base(context)
        {
            this.context = context;
        }
        public LoginRepository() : base(new ApplicationDbContext())
        {
        }

        public Terminal OpeningSave(Guid terminalId, bool isOpen, string userId, decimal openingAmount)
        {
            var _Terminal = context.Terminal.Find(terminalId);
            if (isOpen == false)
            {
                _Terminal.Open(userId, openingAmount);

                var cd = context.CashDrawer.FirstOrDefault(c => c.TerminalId == _Terminal.Id);
                cd.SetOpeningBalance(userId, openingAmount);
                if (cd.Logs != null)
                {
                    context.CashDrawerLog.AddRange(cd.Logs);
                }
                if (_Terminal.StatusLog != null && _Terminal.StatusLog.Count > 0)
                {
                    context.TerminalStatusLog.AddRange(_Terminal.StatusLog);
                }

                context.SaveChanges();
            }

            return _Terminal;


        }

        public Terminal OpenTerminal(Guid terminalId, bool isOpen, string userId)
        {
            var _Terminal = context.Terminal.Find(terminalId);

            if (isOpen == false)
            {
                _Terminal.Open(userId, 0);
                if (_Terminal.StatusLog != null && _Terminal.StatusLog.Count > 0)
                {
                    context.TerminalStatusLog.AddRange(_Terminal.StatusLog);
                }

                context.SaveChanges();
            }

            return _Terminal;


        }

        public bool LoginAndUpdateDallaKey(OutletUser userModel, string DallasKey)
        {

            bool isLogedIn = false;

            string Id = userModel.Id;


            var user = context.OutletUser.FirstOrDefault(u => u.Id == Id);
            if (user != null)
            {
                user.DallasKey = DallasKey;
                string userId = user.UserCode;
                UserLog prevlogedin = context.UserLog.FirstOrDefault(ul => ul.UserId == userId && ul.IsLogedOut == 0 && ul.LogDate == DateTime.Now.Date);
                if (prevlogedin == null)
                {
                    var userLog = new UserLog
                    {
                        UserId = userId,
                        LoginTime = DateTime.Now,
                        IsLogedOut = 0,
                        LogDate = DateTime.Now.Date
                    };
                    context.UserLog.Add(userLog);
                    context.SaveChanges();
                }
                isLogedIn = true;


            }


            return isLogedIn;


        }

        public OutletUser CheckUserKey(string username, string password)
        {
            return context.OutletUser.FirstOrDefault(u => u.UserCode == username && u.Password == password);
        }

        public OutletUser ValidateLogin(string userId, string password, bool isLogedIn)
        {
            var user = context.OutletUser.FirstOrDefault(u => u.UserCode == userId && u.Password == password);
            if (user != null)
            {
                if (isLogedIn)
                {
                    return user;
                }
                DateTime lgDate = DateTime.Now.Date;
                var prevlogedin =
                    context.UserLog.FirstOrDefault(
                        ul => ul.UserId == userId && ul.IsLogedOut == 0 && ul.LogDate == lgDate);

                if (prevlogedin == null)
                {
                    var userLog = new UserLog
                    {
                        UserId = userId,
                        LoginTime = DateTime.Now,
                        IsLogedOut = 0,
                        LogDate = DateTime.Now.Date
                    };
                    context.UserLog.Add(userLog);
                }
                context.SaveChanges();
                return user;
            }
            else
                return null;


        }

        public OutletUser IsRegisterByDallasKey(string dallasKey)
        {
            return context.OutletUser.FirstOrDefault(u => u.DallasKey == dallasKey);
        }

        public bool IsLoggedInByDallasKey(OutletUser user, string userId)
        {

            if (user != null)
            {

                DateTime logDate = DateTime.Now.Date;
                var prevlogedin = context.UserLog.FirstOrDefault(ul => ul.UserId == userId && ul.IsLogedOut == 0 && ul.LogDate == logDate);

                if (prevlogedin == null)
                {
                    var userLog = new UserLog
                    {
                        UserId = userId,
                        LoginTime = DateTime.Now,
                        IsLogedOut = 0,
                        LogDate = DateTime.Now.Date
                    };
                    context.UserLog.Add(userLog);
                    context.SaveChanges();
                }
            }
            return false;

        }
        public string RegisterUser(OutletUser userModel, string strPD, string dallasKey,out OutletUser tillUser)
        {
            tillUser = null;
            string msg = "";
            var user = context.OutletUser.FirstOrDefault(u => u.UserName == userModel.Id);
            if (user != null)
            {

                if (user.Password != strPD)
                {
                    msg = "Error:Invalid password";
                }
                else
                {
                    user.DallasKey = dallasKey;
                    context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                }



            }
            else
            {
                 tillUser = new OutletUser
                 {
                    Id = Guid.NewGuid().ToString(),
                    UserName = userModel.UserName,
                    UserCode = userModel.UserCode,
                    Password = userModel.Password,
                    Email = userModel.Email,
                    DallasKey = userModel.DallasKey,
                    OutletId = userModel.OutletId,
                    Updated = DateTime.Now
                };
                context.OutletUser.Add(tillUser);


            }
            context.SaveChanges();

            return msg;

        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
