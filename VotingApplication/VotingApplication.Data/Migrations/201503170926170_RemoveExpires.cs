namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveExpires : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Polls", "Expires");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Polls", "Expires", c => c.Boolean(nullable: false));
        }
    }
}
