namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_Election_Mode : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Polls", "ElectionMode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Polls", "ElectionMode", c => c.Boolean(nullable: false));
        }
    }
}
