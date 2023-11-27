using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSSUM.Model
{
    public class TablesSyncLog
    {
        public long Id { get; set; }
        public TableName TableName { get; set; }
        public string TableKey { get; set; }
        public Guid OutletId { get; set; }
        public Guid TerminalId { get; set; }

    }
    public enum TableName
    {
        Accounting = 0,
        BongCounter = 1,
        Campaign = 2,
        CashDrawer = 3,
        Category = 4,
        Client = 5,
        Customer=6,
        Customer_CustomField=7,
        CustomerBonus=8,
        CustomerCard=9,
        CustomerCustomField=10,
        CustomerDiscountGroup=11,
        CustomerInvoice=12,
        DiscountGroup=13,
        Employee=14,
        EmployeeLog=15,
        ExceptionLog=16,
        Floor=17,
        FoodTable=18,
        IconStore=19,
        Inbox=20,
        InventoryTask=21,
        InvoiceCounter=22,
        ItemCategory=23,
        ItemInventory=24,
        ItemTransaction=25,
        Journal=26,
        JournalAction=27,
        Language=28,
        MQTTBuffer=29,
        MQTTClient=30,
        OrderDetail=31,
        OrderMaster=32,
        Outlet=33,
        OutletUser=34,
        Payment=35,
        PaymentDeviceLog=36,
        PaymentType=37,
        PricePolicy=38,
        Printer=39,
        Product=40,
        Product_PricePolicy=41,
        Product_Text=42,
        ProductCampaign=43,
        ProductGroup=44,
        ProductPrice=45,
        Receipt=46,
        Report=47,
        ReportData=48,
        Roles=49,
        Setting=50,
        TablesSyncLog=51,
        Tax=52,
        Terminal=53,
        TerminalStatusLog=54,
        UserClaims=55,
        UserLog=56,
        UserLogins=57,
        Users=58,
        UsersInRoles=59,
        VoucherTransaction=60,
        Warehouse=61,
        WarehouseLocation=62,
        ZReportSetting=63



    }
}
