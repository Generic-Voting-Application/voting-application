namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHiddenResults : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Polls", "HiddenResults", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Polls", "HiddenResults");
        }
    }
}
