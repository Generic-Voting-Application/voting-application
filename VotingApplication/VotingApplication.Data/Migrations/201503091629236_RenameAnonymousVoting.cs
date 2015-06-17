namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameAnonymousVoting : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Polls", "NamedVoting", c => c.Boolean(nullable: false));
            DropColumn("dbo.Polls", "AnonymousVoting");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Polls", "AnonymousVoting", c => c.Boolean(nullable: false));
            DropColumn("dbo.Polls", "NamedVoting");
        }
    }
}
