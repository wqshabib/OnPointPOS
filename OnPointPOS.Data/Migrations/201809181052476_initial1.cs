namespace POSSUM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItemStock",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItemId = c.Guid(nullable: false),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BatchNo = c.String(),
                        ExpiryDate = c.DateTime(nullable: false),
                        Updated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.ProductOpening");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ProductOpening",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Guid(nullable: false),
                        OpeningQuantity = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BatchNo = c.String(),
                        ExpiryDate = c.DateTime(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.ItemStock");
        }
    }
}
