namespace POSSUM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveQuantityPrecision : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderDetail", "Qty", c => c.Decimal(nullable: false, precision: 18, scale: 5));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrderDetail", "Qty", c => c.Decimal(nullable: false, precision: 18, scale: 5));
        }
    }
}
