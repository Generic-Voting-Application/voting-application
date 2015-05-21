namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MetricIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Metrics", "Timestamp");
            CreateIndex("dbo.Metrics", "PollId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Metrics", new[] { "PollId" });
            DropIndex("dbo.Metrics", new[] { "Timestamp" });
        }
    }
}
