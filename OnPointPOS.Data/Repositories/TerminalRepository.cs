using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using POSSUM.Model;

namespace POSSUM.Data
{
    public class TerminalRepository
    {
       
        public Terminal GetDefaultTerminal()
        {
            ApplicationDbContext db = new ApplicationDbContext();
            db.Configuration.LazyLoadingEnabled = false;
            return db.Terminal.FirstOrDefault();
        }

        public Terminal GetTerminalById(Guid id)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            db.Configuration.LazyLoadingEnabled = false;
            return db.Terminal.FirstOrDefault(t=>t.Id==id);
        }

        public bool UpdateTerminalCCUData(Guid id, string CCUData)
        {
            using (var db = new ApplicationDbContext())
            {
                var _terminal = db.Terminal.FirstOrDefault(p => p.Id == id);
                if (_terminal != null)
                {
                    _terminal.CCUData = CCUData;
                    _terminal.Updated = DateTime.Now;

                    db.Entry(_terminal).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
            }

            return false;
        }
    }
}
