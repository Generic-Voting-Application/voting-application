namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHasVoted : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Ballots", "HasVoted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Ballots", "HasVoted");
        }
    }
}
