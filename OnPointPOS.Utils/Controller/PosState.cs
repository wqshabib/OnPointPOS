using POSSUM.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Utils.Controller
{
    public class PosState
    {
        private string _localConnectionString;       
        public PosState()
        {
            _localConnectionString = ConfigurationManager.ConnectionStrings["LocalConnection"].ConnectionString;           
        }

        public static PosState GetInstance()
        {
            return new PosState();

        }

        // Return a new UoW (transaction)
       
        public IUnitOfWork CreateLocalIdentityUnitOfWork()
        {
            
            return new UnitOfWork(new ApplicationDbContext(_localConnectionString));
        }
       
      

    }
}
