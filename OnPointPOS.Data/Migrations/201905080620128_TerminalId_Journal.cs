namespace POSSUM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TerminalId_Journal : DbMigration
    {
        public override void Up()
        {
            //DropColumn("dbo.Category", "ImageURL");
            //DropColumn("dbo.Category", "Description");
            //DropColumn("dbo.Customer", "Email");
            //DropColumn("dbo.OrderDetail", "ParentID");
            //DropColumn("dbo.OrderDetail", "ProductName");
            //DropColumn("dbo.OrderMaster", "OrderIntID");
            //DropColumn("dbo.Product", "ImageURL");
            //DropColumn("dbo.Product", "ProductDescription");
        }
        
        public override void Down()
        {
        //    AddColumn("dbo.Product", "ProductDescription", c => c.String());
        //    AddColumn("dbo.Product", "ImageURL", c => c.String());
        //    AddColumn("dbo.OrderMaster", "OrderIntID", c => c.Int(nullable: false));
        //    AddColumn("dbo.OrderDetail", "ProductName", c => c.String());
        //    AddColumn("dbo.OrderDetail", "ParentID", c => c.Guid());
        //    AddColumn("dbo.Customer", "Email", c => c.String());
        //    AddColumn("dbo.Category", "Description", c => c.String());
        //    AddColumn("dbo.Category", "ImageURL", c => c.String());
        }
    }
}
