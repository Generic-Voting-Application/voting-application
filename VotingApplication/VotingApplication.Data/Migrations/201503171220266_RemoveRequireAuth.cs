namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveRequireAuth : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Polls", "RequireAuth");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Polls", "RequireAuth", c => c.Boolean(nullable: false));
        }
    }
}
