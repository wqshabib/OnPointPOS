namespace POSSUM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ab : DbMigration
    {
        public override void Up()
        {
            //AddColumn("dbo.OrderDetail", "OrderLine_Id", c => c.Guid());
            //CreateIndex("dbo.OrderDetail", "OrderLine_Id");
            //AddForeignKey("dbo.OrderDetail", "OrderLine_Id", "dbo.OrderDetail", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrderDetail", "OrderLine_Id", "dbo.OrderDetail");
            DropIndex("dbo.OrderDetail", new[] { "OrderLine_Id" });
            DropColumn("dbo.OrderDetail", "OrderLine_Id");
        }
    }
}
