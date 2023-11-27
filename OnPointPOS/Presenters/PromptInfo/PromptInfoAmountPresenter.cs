using System;
using System.Linq;
using POSSUM.Model;
using POSSUM.Res;
using POSSUM.Data;

namespace POSSUM.Presenters.PromptInfo
{
    public class PromptInfoAmountPresenter
    {
        private readonly IPromptInfoAmountView _view;

        public PromptInfoAmountPresenter(IPromptInfoAmountView view)
        {
            _view = view;
        }

        public void HandleAddCashSave()
        {
            decimal cashAmount = _view.GetCashAmount();
            if (cashAmount == 0)
            {
                _view.ShowError("Can't add cash with 0 amount", "Invalid cash");
                return;
            }

            try
            {
                using (var db = new ApplicationDbContext())
                {
                    //var terminalRepo = new Repository<Terminal, Guid>(uof.Session);
                    //var terminal = terminalRepo.Get(Defaults.Terminal.Id);
                    //terminal.Open(Defaults.User.Id, openingAmount);
                   
                    var cd = db.CashDrawer.FirstOrDefault(c => c.TerminalId == Defaults.Terminal.Id);
                    if (cd != null)
                    {
                        cd.SetCashAdded(Defaults.User.Id, cashAmount);
                        if (cd.Logs != null)
                            db.CashDrawerLog.AddRange(cd.Logs);
                    }

                    db.SaveChanges();
                }
                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OpenCashDrawer));
                _view.SuccessMessage(UI.Main_Amount + " " + UI.Main_Deposit + " " + UI.CheckOutOrder_Done);
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message, Defaults.AppProvider.AppTitle);
            }
            // Set terminalstatus

            // Log cashdrawer
        }

        public void HandleDropCashSave()
        {
            decimal cashAmount = _view.GetCashAmount();

            if (cashAmount == 0)
            {
                _view.ShowError("Can't drop cash with 0 amount", "Invalid Cash");
                return;
            }

            try
            {
                using (var db = new ApplicationDbContext())
                {
                    //var terminalRepo = new Repository<Terminal, Guid>(uof.Session);
                    //var terminal = terminalRepo.Get(Defaults.Terminal.Id);
                    //terminal.Open(Defaults.User.Id, openingAmount);
                  
                    var cd = db.CashDrawer.FirstOrDefault(c => c.TerminalId == Defaults.Terminal.Id);
                    if (cd != null)
                    {
                        cd.SetCashDropped(Defaults.User.Id, cashAmount);
                        if (cd.Logs != null)
                            db.CashDrawerLog.AddRange(cd.Logs);
                    }

                    db.SaveChanges();
                }

                LogWriter.JournalLog(Convert.ToInt16(JournalActionCode.OpenCashDrawer));
                _view.SuccessMessage(UI.Main_Amount + " " + UI.Main_Withdrawal + " " + UI.CheckOutOrder_Done);
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message, Defaults.AppProvider.AppTitle);
            }
            // Set terminalstatus

            // Log cashdrawer
        }
    }
}