namespace POSSUM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemveOrderItemList : DbMigration
    {
        public override void Up()
        {
            //DropForeignKey("dbo.OrderDetail", "OrderLine_Id", "dbo.OrderDetail");
            //DropIndex("dbo.OrderDetail", new[] { "OrderLine_Id" });
            //DropColumn("dbo.OrderDetail", "OrderLine_Id");
        }
        
        public override void Down()
        {
            //AddColumn("dbo.OrderDetail", "OrderLine_Id", c => c.Guid());
            //CreateIndex("dbo.OrderDetail", "OrderLine_Id");
            //AddForeignKey("dbo.OrderDetail", "OrderLine_Id", "dbo.OrderDetail", "Id");
        }
    }
}
