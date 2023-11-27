using POSSUM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Data
{
    public class SettingRepository
    {
        public List<Setting> GetReceiptBankSettings()
        {

            List<Setting> settings = new List<Setting>();
            using (var db = new ApplicationDbContext())
            {
                var accountNumber = db.Setting.FirstOrDefault(ot => ot.Code == SettingCode.AccountNumber);
                if (accountNumber != null)
                    settings.Add(accountNumber);
                else
                    settings.Add(new Setting { Code = SettingCode.AccountNumber, Value = " " });
                var Payee = db.Setting.FirstOrDefault(ot => ot.Code == SettingCode.PaymentReceiver);
                if (Payee != null)
                    settings.Add(Payee);
                else
                    settings.Add(new Setting { Code = SettingCode.PaymentReceiver, Value = " " });

                return settings;
            }

        }


        public Setting GetSettings(string code)
        {

            using (var db = new ApplicationDbContext())
            {

                return db.Setting.FirstOrDefault(i => i.Code.Equals(code));
            }

        }
        public string GetSettingsByCode(SettingCode code,string terminalId)
        {
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var terminal = Guid.Parse(terminalId);
                    var res = db.Setting.FirstOrDefault(i => i.Code == code && i.TerminalId == terminal).Value;
                    return res;
                }
            }
            catch (Exception ex)
            {

                return ex.ToString();
            }

            

        }
        public Setting GetDefaultSettings(SettingCode code, string terminlId)
        {

            using (var db = new ApplicationDbContext())
            {
                var terminal = Guid.Parse(terminlId);
                return db.Setting.FirstOrDefault(i => i.Code == code && i.TerminalId == terminal);
            }

        }





        public bool SaveSettings(List<Setting> lst,Guid terminalGuid)
        {

            using (var db = new ApplicationDbContext())
            {

                foreach (var item in lst)
                {
                    var setting = db.Setting.FirstOrDefault(s => s.Code == item.Code && s.TerminalId== terminalGuid);
                    if (setting != null)
                    { 
                        setting.Value = item.Value;
                        setting.Updated = item.Updated;
                    }
                    else
                        db.Setting.Add(item);
                }
                db.SaveChanges();


                return true;
            }

        }

        public List<Printer> GetPrinters(string keyword)
        {
            var printerList = new List<Printer>();

            using (var db = new ApplicationDbContext())
            {

                if (string.IsNullOrEmpty(keyword))
                {
                    printerList = db.Printer.ToList();
                }
                else
                {
                    keyword = keyword.ToLower();
                    printerList =
                        db.Printer.Where(
                            p => p.LocationName.ToLower() == keyword || p.PrinterName.ToLower() == keyword).ToList();
                }

            }
            return printerList;
        }

        public bool UpdatePrinter(Printer printer)
        {

            using (var db = new ApplicationDbContext())
            {

                if (printer.Id > 0)
                {
                    var _printer = db.Printer.FirstOrDefault(p => p.Id == printer.Id);
                    _printer.LocationName = printer.LocationName;
                    _printer.PrinterName = printer.PrinterName;
                    _printer.Updated = DateTime.Now;
                    db.Entry(_printer).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    int lastId = 0;
                    try
                    {
                        lastId = db.Printer.Max(p => p.Id);
                        lastId = lastId + 1;
                    }
                    catch
                    {
                        lastId = 1;
                    }
                    printer.Id = lastId;

                    db.Printer.Add(printer);
                }
                db.SaveChanges();
                return true;
            }
        }

    }
}
