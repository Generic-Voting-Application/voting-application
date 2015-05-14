namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullablePointsConfig : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Polls", "MaxPoints", c => c.Int());
            AlterColumn("dbo.Polls", "MaxPerVote", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Polls", "MaxPerVote", c => c.Int(nullable: false));
            AlterColumn("dbo.Polls", "MaxPoints", c => c.Int(nullable: false));
        }
    }
}
