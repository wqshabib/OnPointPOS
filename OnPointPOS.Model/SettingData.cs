using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class SettingData
    {
        public List<TempSetting> Settings { get; set; }
        public TempTerminal Terminal { get; set; }
        public TempOutlet Outlet { get; set; }
        public List<TempPrinter> Printers { get; set; }
        public List<TempCampaign> Campaigns { get; set; }
        public List<ZReportSetting> ZReportSettings { get; set; }
        public List<Client> Clients { get; set; }
        public List<Inbox> InboxMessages { get; set; }


    }


    public class TempCampaign
    {

        public virtual int Id { get; set; }
        public virtual int BuyLimit { get; set; }
        public virtual int FreeOffer { get; set; }
        public virtual string Description { get; set; }
        public virtual DateTime? Updated { get; set; }
    }
    /// <summary>
    /// Purpose of following temp classed just to serialize between sync service and sync API
    /// </summary>
    public class TempTerminal
    {

        public virtual Guid Id { get; set; }
        public virtual Guid OutletId { get; set; }
        public virtual int TerminalNo { get; set; }
        public virtual Guid TerminalType { get; set; }
        public virtual string UniqueIdentification { get; set; }
        public virtual string HardwareAddress { get; set; }
        public virtual string Description { get; set; }
        public virtual Terminal.TerminalStatus Status { get; set; }
        public virtual IList<TempCashDrawer> CashDrawer { get; set; }
        public virtual int CategoryId { get; set; }
        public virtual bool IsDeleted { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Updated { get; set; }
    }
    public class TempCashDrawer
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Location { get; set; }
        public virtual string UserId { get; set; }
        public virtual Guid TerminalId { get; set; }
        public virtual string ConnectionString { get; set; }
    }
    public class TempOutlet
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Address1 { get; set; }
        public virtual string Address2 { get; set; }
        public virtual string Address3 { get; set; }
        public virtual string City { get; set; }
        public virtual string PostalCode { get; set; }
        public virtual int BillPrinterId { get; set; }
        public virtual int KitchenPrinterId { get; set; }
        public virtual bool IsDeleted { get; set; }

        public virtual string Email { get; set; }
        public virtual string WebUrl { get; set; }
        public virtual string Phone { get; set; }
        public virtual string OrgNo { get; set; }
        public virtual string HeaderText { get; set; }
        public virtual string FooterText { get; set; }
        public virtual string TaxDescription { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Updated { get; set; }
        public virtual Guid WarehouseID { get; set; }
    }

    public class TempSetting
    {
        public virtual int Id { get; set; }
        //  public virtual string Code { get; set; }
        public virtual SettingCode Code { get; set; }
        public virtual string Value { get; set; }
        public virtual SettingType Type { get; set; }
        public virtual Guid TerminalId { get; set; }
        public virtual Guid OutletId { get; set; }
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual int Sort { get; set; }
        public virtual DateTime Created { get; set; }
        public virtual DateTime? Updated { get; set; }
    }
    public class TempPrinter
    {
        public virtual int Id { get; set; }
        public virtual string LocationName { get; set; }
        public virtual string PrinterName { get; set; }
        public virtual DateTime? Updated { get; set; }
    }
}
