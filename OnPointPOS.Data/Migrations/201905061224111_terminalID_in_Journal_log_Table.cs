namespace POSSUM.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class terminalID_in_Journal_log_Table : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Journal", "TerminalId", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Journal", "TerminalId");
        }
    }
}
