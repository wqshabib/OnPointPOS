namespace POSSUM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropcolumnOrderLie_Id : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.OrderDetail", "OrderLine_Id", "dbo.OrderDetail");
            DropIndex("dbo.OrderDetail", new[] { "OrderLine_Id" });
            DropColumn("dbo.OrderDetail", "OrderLine_Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderDetail", "OrderLine_Id", "dbo.OrderDetail");
            DropIndex("dbo.OrderDetail", new[] { "OrderLine_Id" });
            DropColumn("dbo.OrderDetail", "OrderLine_Id");
        }
    }
}
