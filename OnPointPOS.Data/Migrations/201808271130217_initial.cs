namespace POSSUM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounting",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        AcNo = c.Int(nullable: false),
                        Name = c.String(maxLength: 50),
                        IsDeleted = c.Boolean(nullable: false),
                        TAX = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Updated = c.DateTime(nullable: false),
                        SortOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BongCounter",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Counter = c.Int(nullable: false),
                        BarCounter = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Campaign",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        BuyLimit = c.Int(nullable: false),
                        FreeOffer = c.Int(nullable: false),
                        Description = c.String(),
                        Updated = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CashDrawer",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Location = c.String(),
                        UserId = c.String(),
                        TerminalId = c.Guid(nullable: false),
                        ConnectionString = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CashDrawerLog",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CashDrawerId = c.Guid(nullable: false),
                        ActivityDate = c.DateTime(nullable: false),
                        ActivityType = c.Int(nullable: false),
                        UserId = c.String(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OrderId = c.Guid(nullable: false),
                        Synced = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CashDrawer", t => t.CashDrawerId, cascadeDelete: true)
                .Index(t => t.CashDrawerId);
            
            CreateTable(
                "dbo.Category",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(maxLength: 50),
                        Parant = c.Int(nullable: false),
                        CategoryLevel = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(),
                        ColorCode = c.String(),
                        SortOrder = c.Int(nullable: false),
                        IconId = c.Int(),
                        ReportOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Client",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        clientId = c.String(),
                        clientRequestTimeout = c.Long(),
                        password = c.String(),
                        connectionString = c.String(),
                        ClientUserId = c.String(),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Customer",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 150),
                        OrgNo = c.String(maxLength: 50),
                        Address1 = c.String(maxLength: 250),
                        Address2 = c.String(maxLength: 250),
                        FloorNo = c.Int(nullable: false),
                        PortCode = c.Int(nullable: false),
                        CustomerNo = c.Int(nullable: false),
                        City = c.String(maxLength: 50),
                        Phone = c.String(maxLength: 50),
                        ZipCode = c.String(maxLength: 50),
                        Reference = c.String(maxLength: 50),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(),
                        DirectPrint = c.Boolean(nullable: false),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Customer_CustomField",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FieldId = c.Guid(nullable: false),
                        CustomerId = c.Guid(nullable: false),
                        Caption = c.String(),
                        Text = c.String(),
                        SortOrder = c.Int(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.CustomerCustomField", t => t.FieldId, cascadeDelete: true)
                .Index(t => t.FieldId)
                .Index(t => t.CustomerId);
            
            CreateTable(
                "dbo.CustomerCustomField",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Caption = c.String(),
                        SortOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CustomerBonus",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CustomerId = c.Guid(nullable: false),
                        OrderId = c.Guid(nullable: false),
                        OutletId = c.Guid(nullable: false),
                        ChangeValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CurrentSum = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CustomerCard",
                c => new
                    {
                        CardId = c.Guid(nullable: false),
                        UniqueId = c.String(nullable: false, maxLength: 200),
                        CustomerId = c.Guid(nullable: false),
                        Title = c.String(),
                        Active = c.Boolean(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.CardId);
            
            CreateTable(
                "dbo.CustomerDiscountGroup",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DiscountId = c.Guid(nullable: false),
                        CustomerId = c.Guid(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CustomerInvoice",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        InvoiceNumber = c.Long(nullable: false),
                        CustomerId = c.Guid(nullable: false),
                        Remarks = c.String(maxLength: 250),
                        InvoiceTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreationDate = c.DateTime(nullable: false),
                        Synced = c.Boolean(nullable: false),
                        PaymentStatus = c.Int(nullable: false),
                        PaidDate = c.DateTime(),
                        DueDate = c.DateTime(),
                        OutletId = c.Guid(nullable: false),
                        TerminalId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DiscountGroup",
                c => new
                    {
                        DiscountId = c.Guid(nullable: false),
                        Title = c.String(),
                        Discount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.DiscountId);
            
            CreateTable(
                "dbo.Employee",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        SSNO = c.String(maxLength: 50),
                        Email = c.String(maxLength: 150),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EmployeeLog",
                c => new
                    {
                        LogId = c.Guid(nullable: false),
                        EmployeeId = c.Guid(nullable: false),
                        CheckIn = c.DateTime(),
                        CheckOut = c.DateTime(),
                        Completed = c.Boolean(nullable: false),
                        Synced = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LogId)
                .ForeignKey("dbo.Employee", t => t.EmployeeId, cascadeDelete: true)
                .Index(t => t.EmployeeId);
            
            CreateTable(
                "dbo.ExceptionLog",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ExceptiontText = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        TerminalId = c.Guid(nullable: false),
                        Synced = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Floor",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FoodTable",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 50),
                        FloorId = c.Int(nullable: false),
                        PositionX = c.Int(nullable: false),
                        PositionY = c.Int(nullable: false),
                        Height = c.Int(nullable: false),
                        Width = c.Int(nullable: false),
                        Chairs = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        ImageUrl = c.String(),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Floor", t => t.FloorId, cascadeDelete: true)
                .Index(t => t.FloorId);
            
            CreateTable(
                "dbo.IconStore",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        Title = c.String(),
                        Photo = c.Binary(),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Inbox",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Description = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        IsRead = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InventoryTask",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Priority = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        StatusMessage = c.String(),
                        Created = c.DateTime(nullable: false),
                        Executed = c.DateTime(),
                        Processed = c.DateTime(),
                        ItemId = c.Guid(),
                        OrderGuid = c.Guid(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.InvoiceCounter",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        LastNo = c.String(),
                        InvoiceType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItemCategory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemId = c.Guid(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        IsPrimary = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ItemInventory",
                c => new
                    {
                        ItemInventoryID = c.Guid(nullable: false),
                        ItemId = c.Guid(nullable: false),
                        WarehouseID = c.Guid(nullable: false),
                        WarehouseLocationID = c.Guid(nullable: false),
                        StockCount = c.Int(nullable: false),
                        StockReservations = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ItemInventoryID);
            
            CreateTable(
                "dbo.ItemTransaction",
                c => new
                    {
                        ItemTransactionID = c.Guid(nullable: false),
                        ItemID = c.Guid(nullable: false),
                        OutletID = c.Guid(nullable: false),
                        TerminalID = c.Guid(nullable: false),
                        OrderID = c.Guid(nullable: false),
                        WarehouseID = c.Guid(nullable: false),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Direction = c.Int(nullable: false),
                        TransactionDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ItemTransactionID);
            
            CreateTable(
                "dbo.Journal",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UserId = c.String(),
                        OrderId = c.Guid(),
                        ItemId = c.Guid(),
                        TableId = c.Int(),
                        ActionId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        LogMessage = c.String(maxLength: 150),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.JournalAction",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Type = c.String(),
                        ActionCode = c.String(),
                        Description = c.String(),
                        Description2 = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Language",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Code = c.String(),
                        IsDefault = c.Boolean(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MQTTBuffer",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        MessageId = c.Guid(nullable: false),
                        ClientId = c.Guid(nullable: false),
                        OrderId = c.Guid(),
                        Action = c.String(),
                        JsonData = c.String(),
                        Created = c.DateTime(nullable: false),
                        Status = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MQTTClient",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ClientId = c.Guid(nullable: false),
                        Name = c.String(),
                        LastPing = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OrderDetail",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        OrderId = c.Guid(nullable: false),
                        ItemId = c.Guid(nullable: false),
                        Qty = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountedUnitPrice = c.Decimal(nullable: false, precision: 8, scale: 2),
                        PurchasePrice = c.Decimal(nullable: false, precision: 8, scale: 2),
                        ItemDiscount = c.Decimal(nullable: false, precision: 8, scale: 2),
                        TaxPercent = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Active = c.Int(nullable: false),
                        ItemComments = c.String(maxLength: 150),
                        UnitPrice = c.Decimal(nullable: false, precision: 8, scale: 2),
                        DiscountPercentage = c.Decimal(nullable: false, precision: 8, scale: 2),
                        UnitsInPackage = c.Int(nullable: false),
                        ItemStatus = c.Int(nullable: false),
                        IsCoupon = c.Int(nullable: false),
                        Direction = c.Int(nullable: false),
                        DiscountType = c.Int(nullable: false),
                        DiscountDescription = c.String(),
                        ItemType = c.Int(nullable: false),
                        GroupId = c.Guid(nullable: false),
                        GroupKey = c.Guid(),
                        IngredientMode = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OrderMaster", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.Product", t => t.ItemId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.ItemId);
            
            CreateTable(
                "dbo.OrderMaster",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TableId = c.Int(nullable: false),
                        CustomerId = c.Guid(nullable: false),
                        CreationDate = c.DateTime(nullable: false),
                        OrderTotal = c.Decimal(nullable: false, precision: 12, scale: 2),
                        OrderNoOfDay = c.String(maxLength: 20),
                        Status = c.Int(nullable: false),
                        PaymentStatus = c.Int(nullable: false),
                        Updated = c.Int(nullable: false),
                        UserId = c.String(),
                        TaxPercent = c.Decimal(nullable: false, precision: 18, scale: 2),
                        InvoiceNumber = c.String(maxLength: 20),
                        InvoiceDate = c.DateTime(),
                        InvoiceGenerated = c.Int(nullable: false),
                        TipAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Comments = c.String(maxLength: 150),
                        ShiftNo = c.Int(nullable: false),
                        ShiftOrderNo = c.Int(nullable: false),
                        ShiftClosed = c.Int(nullable: false),
                        TipAmountType = c.Int(nullable: false),
                        ZPrinted = c.Int(nullable: false),
                        CheckOutUserId = c.String(maxLength: 40),
                        OrderComments = c.String(maxLength: 50),
                        Type = c.Int(nullable: false),
                        CustomerInvoiceId = c.Guid(),
                        Bong = c.String(),
                        TerminalId = c.Guid(nullable: false),
                        OutletId = c.Guid(nullable: false),
                        TrainingMode = c.Boolean(nullable: false),
                        RoundedAmount = c.Decimal(nullable: false, precision: 8, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Outlet", t => t.OutletId, cascadeDelete: true)
                .Index(t => t.OutletId);
            
            CreateTable(
                "dbo.Outlet",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        Address3 = c.String(),
                        City = c.String(),
                        PostalCode = c.String(),
                        BillPrinterId = c.Int(nullable: false),
                        KitchenPrinterId = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        Email = c.String(),
                        WebUrl = c.String(),
                        Phone = c.String(),
                        OrgNo = c.String(),
                        HeaderText = c.String(),
                        FooterText = c.String(),
                        TaxDescription = c.String(),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(),
                        WarehouseID = c.Guid(nullable: false),
                        UniqueCode = c.String(),
                        Active = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Payment",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        OrderId = c.Guid(nullable: false),
                        PaymentType = c.Int(nullable: false),
                        PaidAmount = c.Decimal(nullable: false, precision: 12, scale: 2),
                        ReturnAmount = c.Decimal(nullable: false, precision: 8, scale: 2),
                        PaymentDate = c.DateTime(nullable: false),
                        PaymentRef = c.String(),
                        CreditCardNo = c.String(),
                        TipAmount = c.Decimal(nullable: false, precision: 8, scale: 2),
                        CashCollected = c.Decimal(nullable: false, precision: 8, scale: 2),
                        CashChange = c.Decimal(nullable: false, precision: 8, scale: 2),
                        IsCashSaleDropped = c.Int(nullable: false),
                        Direction = c.Int(nullable: false),
                        ProductName = c.String(),
                        DeviceTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.OrderMaster", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Receipt",
                c => new
                    {
                        ReceiptId = c.Guid(nullable: false),
                        TerminalId = c.Guid(nullable: false),
                        TerminalNo = c.Int(nullable: false),
                        ReceiptNumber = c.Long(nullable: false),
                        OrderId = c.Guid(nullable: false),
                        ReceiptCopies = c.Byte(nullable: false),
                        GrossAmount = c.Decimal(nullable: false, precision: 8, scale: 2),
                        VatAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VatDetail = c.String(),
                        PrintDate = c.DateTime(nullable: false),
                        ControlUnitName = c.String(),
                        ControlUnitCode = c.String(),
                        CustomerPaymentReceipt = c.String(),
                        MerchantPaymentReceipt = c.String(),
                        IsSignature = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ReceiptId)
                .ForeignKey("dbo.OrderMaster", t => t.OrderId, cascadeDelete: true)
                .Index(t => t.OrderId);
            
            CreateTable(
                "dbo.Product",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Description = c.String(maxLength: 150),
                        SKU = c.String(maxLength: 50),
                        BarCode = c.String(maxLength: 50),
                        PLU = c.String(maxLength: 50),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PurchasePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Tax = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Unit = c.Int(nullable: false),
                        AskPrice = c.Boolean(nullable: false),
                        AskWeight = c.Boolean(nullable: false),
                        PriceLock = c.Boolean(nullable: false),
                        ColorCode = c.String(maxLength: 10),
                        PrinterId = c.Int(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        PlaceHolder = c.Boolean(nullable: false),
                        AskVolume = c.Boolean(nullable: false),
                        NeedIngredient = c.Boolean(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(),
                        Bong = c.Boolean(nullable: false),
                        Sticky = c.Int(nullable: false),
                        Seamless = c.Boolean(nullable: false),
                        ShowItemButton = c.Boolean(nullable: false),
                        AccountingId = c.Int(nullable: false),
                        ReceiptMethod = c.Int(nullable: false),
                        ItemType = c.Int(nullable: false),
                        DiscountAllowed = c.Boolean(nullable: false),
                        PreparationTime = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.OutletUser",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        OutletId = c.Guid(nullable: false),
                        Email = c.String(),
                        UserCode = c.String(),
                        UserName = c.String(),
                        Password = c.String(),
                        TrainingMode = c.Boolean(nullable: false),
                        Active = c.Boolean(nullable: false),
                        DallasKey = c.String(),
                        Updated = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PaymentDeviceLog",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OrderId = c.Guid(nullable: false),
                        OrderTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        VatAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CashBack = c.Decimal(nullable: false, precision: 18, scale: 2),
                        SendDate = c.DateTime(nullable: false),
                        Synced = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PaymentType",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(),
                        SwedishName = c.String(),
                        AccountingCode = c.Int(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PricePolicy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BuyLimit = c.Int(nullable: false),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Description = c.String(),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Printer",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        LocationName = c.String(),
                        PrinterName = c.String(),
                        TerminalId = c.Guid(),
                        IPAddress = c.String(),
                        Updated = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Product_PricePolicy",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemId = c.Guid(nullable: false),
                        BuyLimit = c.Int(nullable: false),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Active = c.Boolean(nullable: false),
                        Deleted = c.Boolean(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Product_Text",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemId = c.Guid(nullable: false),
                        LanguageId = c.Int(nullable: false),
                        TextDescription = c.String(),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProductCampaign",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ItemId = c.Guid(nullable: false),
                        CampaignId = c.Int(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Active = c.Boolean(nullable: false),
                        Updated = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProductGroup",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ItemId = c.Guid(nullable: false),
                        GroupId = c.Guid(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProductPrice",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemId = c.Guid(nullable: false),
                        PurchasePrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        OutletId = c.Guid(nullable: false),
                        PriceMode = c.Int(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Report",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CreationDate = c.DateTime(nullable: false),
                        ReportType = c.Int(nullable: false),
                        ReportNumber = c.Int(nullable: false),
                        TerminalId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Terminal", t => t.TerminalId, cascadeDelete: true)
                .Index(t => t.TerminalId);
            
            CreateTable(
                "dbo.Terminal",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        OutletId = c.Guid(nullable: false),
                        TerminalNo = c.Int(nullable: false),
                        TerminalType = c.Guid(nullable: false),
                        UniqueIdentification = c.String(),
                        HardwareAddress = c.String(),
                        Description = c.String(),
                        Status = c.Int(nullable: false),
                        RootCategoryId = c.Int(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.RootCategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Outlet", t => t.OutletId, cascadeDelete: true)
                .Index(t => t.OutletId)
                .Index(t => t.RootCategoryId);
            
            CreateTable(
                "dbo.ReportData",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ReportId = c.Guid(nullable: false),
                        DataType = c.String(),
                        TextValue = c.String(),
                        ForeignId = c.Int(),
                        Value = c.Decimal(precision: 18, scale: 2),
                        TaxPercent = c.Decimal(precision: 18, scale: 2),
                        DateValue = c.DateTime(),
                        SortOrder = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.UsersInRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                        IdentityUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.IdentityUser_Id)
                .Index(t => t.RoleId)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.Setting",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(),
                        Type = c.Int(nullable: false),
                        Code = c.Int(nullable: false),
                        Value = c.String(),
                        TerminalId = c.Guid(nullable: false),
                        OutletId = c.Guid(nullable: false),
                        Sort = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Updated = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TablesSyncLog",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        TableName = c.Int(nullable: false),
                        TableKey = c.String(),
                        OutletId = c.Guid(nullable: false),
                        TerminalId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tax",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        TaxValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        AccountingCode = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.TerminalStatusLog",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TerminalId = c.Guid(nullable: false),
                        ActivityDate = c.DateTime(nullable: false),
                        UserId = c.String(),
                        ReportId = c.Guid(nullable: false),
                        Status = c.Int(nullable: false),
                        Synced = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Terminal", t => t.TerminalId, cascadeDelete: true)
                .Index(t => t.TerminalId);
            
            CreateTable(
                "dbo.UserLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        LoginTime = c.DateTime(),
                        LogOutTime = c.DateTime(),
                        LogDate = c.DateTime(),
                        IsLogedOut = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        PasswordHash = c.String(maxLength: 70),
                        SecurityStamp = c.String(maxLength: 40),
                        PhoneNumber = c.String(maxLength: 20),
                        UserName = c.String(nullable: false, maxLength: 256),
                        Password = c.String(),
                        TrainingMode = c.Boolean(),
                        Active = c.Boolean(),
                        Created = c.DateTime(),
                        Updated = c.DateTime(),
                        DallasKey = c.String(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                        IdentityUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.UserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                        IdentityUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.Users", t => t.IdentityUser_Id)
                .Index(t => t.IdentityUser_Id);
            
            CreateTable(
                "dbo.VoucherTransaction",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        OrderId = c.Guid(nullable: false),
                        TransactionDate = c.DateTime(nullable: false),
                        ErsReference = c.String(),
                        Canceled = c.Boolean(nullable: false),
                        Product_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Product", t => t.Product_Id)
                .Index(t => t.Product_Id);
            
            CreateTable(
                "dbo.Warehouse",
                c => new
                    {
                        WarehouseID = c.Guid(nullable: false),
                        Alias = c.String(),
                        Name = c.String(),
                        Address1 = c.String(),
                        Address2 = c.String(),
                        Address3 = c.String(),
                        Zipcode = c.String(),
                        City = c.String(),
                        Country = c.String(),
                    })
                .PrimaryKey(t => t.WarehouseID);
            
            CreateTable(
                "dbo.WarehouseLocation",
                c => new
                    {
                        WarehouseLocationID = c.Guid(nullable: false),
                        WarehouseID = c.Guid(nullable: false),
                        Name = c.String(),
                        Path = c.String(),
                    })
                .PrimaryKey(t => t.WarehouseLocationID);
            
            CreateTable(
                "dbo.ZReportSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReportTag = c.String(),
                        Visiblity = c.Boolean(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UsersInRoles", "IdentityUser_Id", "dbo.Users");
            DropForeignKey("dbo.UserLogins", "IdentityUser_Id", "dbo.Users");
            DropForeignKey("dbo.UserClaims", "IdentityUser_Id", "dbo.Users");
            DropForeignKey("dbo.VoucherTransaction", "Product_Id", "dbo.Product");
            DropForeignKey("dbo.TerminalStatusLog", "TerminalId", "dbo.Terminal");
            DropForeignKey("dbo.UsersInRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Report", "TerminalId", "dbo.Terminal");
            DropForeignKey("dbo.Terminal", "OutletId", "dbo.Outlet");
            DropForeignKey("dbo.Terminal", "RootCategoryId", "dbo.Category");
            DropForeignKey("dbo.OrderDetail", "ItemId", "dbo.Product");
            DropForeignKey("dbo.Receipt", "OrderId", "dbo.OrderMaster");
            DropForeignKey("dbo.Payment", "OrderId", "dbo.OrderMaster");
            DropForeignKey("dbo.OrderMaster", "OutletId", "dbo.Outlet");
            DropForeignKey("dbo.OrderDetail", "OrderId", "dbo.OrderMaster");
            DropForeignKey("dbo.FoodTable", "FloorId", "dbo.Floor");
            DropForeignKey("dbo.EmployeeLog", "EmployeeId", "dbo.Employee");
            DropForeignKey("dbo.Customer_CustomField", "FieldId", "dbo.CustomerCustomField");
            DropForeignKey("dbo.Customer_CustomField", "CustomerId", "dbo.Customer");
            DropForeignKey("dbo.CashDrawerLog", "CashDrawerId", "dbo.CashDrawer");
            DropIndex("dbo.VoucherTransaction", new[] { "Product_Id" });
            DropIndex("dbo.UserLogins", new[] { "IdentityUser_Id" });
            DropIndex("dbo.UserClaims", new[] { "IdentityUser_Id" });
            DropIndex("dbo.Users", "UserNameIndex");
            DropIndex("dbo.TerminalStatusLog", new[] { "TerminalId" });
            DropIndex("dbo.UsersInRoles", new[] { "IdentityUser_Id" });
            DropIndex("dbo.UsersInRoles", new[] { "RoleId" });
            DropIndex("dbo.Roles", "RoleNameIndex");
            DropIndex("dbo.Terminal", new[] { "RootCategoryId" });
            DropIndex("dbo.Terminal", new[] { "OutletId" });
            DropIndex("dbo.Report", new[] { "TerminalId" });
            DropIndex("dbo.Receipt", new[] { "OrderId" });
            DropIndex("dbo.Payment", new[] { "OrderId" });
            DropIndex("dbo.OrderMaster", new[] { "OutletId" });
            DropIndex("dbo.OrderDetail", new[] { "ItemId" });
            DropIndex("dbo.OrderDetail", new[] { "OrderId" });
            DropIndex("dbo.FoodTable", new[] { "FloorId" });
            DropIndex("dbo.EmployeeLog", new[] { "EmployeeId" });
            DropIndex("dbo.Customer_CustomField", new[] { "CustomerId" });
            DropIndex("dbo.Customer_CustomField", new[] { "FieldId" });
            DropIndex("dbo.CashDrawerLog", new[] { "CashDrawerId" });
            DropTable("dbo.ZReportSetting");
            DropTable("dbo.WarehouseLocation");
            DropTable("dbo.Warehouse");
            DropTable("dbo.VoucherTransaction");
            DropTable("dbo.UserLogins");
            DropTable("dbo.UserClaims");
            DropTable("dbo.Users");
            DropTable("dbo.UserLog");
            DropTable("dbo.TerminalStatusLog");
            DropTable("dbo.Tax");
            DropTable("dbo.TablesSyncLog");
            DropTable("dbo.Setting");
            DropTable("dbo.UsersInRoles");
            DropTable("dbo.Roles");
            DropTable("dbo.ReportData");
            DropTable("dbo.Terminal");
            DropTable("dbo.Report");
            DropTable("dbo.ProductPrice");
            DropTable("dbo.ProductGroup");
            DropTable("dbo.ProductCampaign");
            DropTable("dbo.Product_Text");
            DropTable("dbo.Product_PricePolicy");
            DropTable("dbo.Printer");
            DropTable("dbo.PricePolicy");
            DropTable("dbo.PaymentType");
            DropTable("dbo.PaymentDeviceLog");
            DropTable("dbo.OutletUser");
            DropTable("dbo.Product");
            DropTable("dbo.Receipt");
            DropTable("dbo.Payment");
            DropTable("dbo.Outlet");
            DropTable("dbo.OrderMaster");
            DropTable("dbo.OrderDetail");
            DropTable("dbo.MQTTClient");
            DropTable("dbo.MQTTBuffer");
            DropTable("dbo.Language");
            DropTable("dbo.JournalAction");
            DropTable("dbo.Journal");
            DropTable("dbo.ItemTransaction");
            DropTable("dbo.ItemInventory");
            DropTable("dbo.ItemCategory");
            DropTable("dbo.InvoiceCounter");
            DropTable("dbo.InventoryTask");
            DropTable("dbo.Inbox");
            DropTable("dbo.IconStore");
            DropTable("dbo.FoodTable");
            DropTable("dbo.Floor");
            DropTable("dbo.ExceptionLog");
            DropTable("dbo.EmployeeLog");
            DropTable("dbo.Employee");
            DropTable("dbo.DiscountGroup");
            DropTable("dbo.CustomerInvoice");
            DropTable("dbo.CustomerDiscountGroup");
            DropTable("dbo.CustomerCard");
            DropTable("dbo.CustomerBonus");
            DropTable("dbo.CustomerCustomField");
            DropTable("dbo.Customer_CustomField");
            DropTable("dbo.Customer");
            DropTable("dbo.Client");
            DropTable("dbo.Category");
            DropTable("dbo.CashDrawerLog");
            DropTable("dbo.CashDrawer");
            DropTable("dbo.Campaign");
            DropTable("dbo.BongCounter");
            DropTable("dbo.Accounting");
        }
    }
}
