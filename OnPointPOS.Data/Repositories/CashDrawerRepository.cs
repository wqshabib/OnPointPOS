using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class CashDrawerRepository
    {
        public void OpenCashDrawer(Guid terminalId, decimal amount,string userId)
        {
            
                using (var db = new ApplicationDbContext())
                {

                    var cashdrawer = db.CashDrawer.First(x => x.TerminalId == terminalId);

                    // Log that drawer was opened
                    cashdrawer.OpenCashDrawerSale(amount, userId);
                    if (cashdrawer.Logs != null)
                        db.CashDrawerLog.AddRange(cashdrawer.Logs);                   

                    // Save log
                    db.SaveChanges();
                }
                
           
        }

    }
}
