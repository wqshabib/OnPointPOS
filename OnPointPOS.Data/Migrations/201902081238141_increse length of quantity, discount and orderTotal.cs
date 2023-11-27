namespace POSSUM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class increselengthofquantitydiscountandorderTotal : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderDetail", "Qty", c => c.Decimal(nullable: false, precision: 18, scale: 10));
            AlterColumn("dbo.OrderDetail", "ItemDiscount", c => c.Decimal(nullable: false, precision: 18, scale: 10));
            AlterColumn("dbo.OrderMaster", "OrderTotal", c => c.Decimal(nullable: false, precision: 18, scale: 10));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrderMaster", "OrderTotal", c => c.Decimal(nullable: false, precision: 12, scale: 2));
            AlterColumn("dbo.OrderDetail", "ItemDiscount", c => c.Decimal(nullable: false, precision: 8, scale: 2));
            AlterColumn("dbo.OrderDetail", "Qty", c => c.Decimal(nullable: false, precision: 18, scale: 5));
        }
    }
}
