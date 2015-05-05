namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMetricTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Metrics",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Timestamp = c.DateTime(nullable: false),
                        MetricType = c.Int(nullable: false),
                        PollId = c.Guid(nullable: false),
                        StatusCode = c.Int(nullable: false),
                        Value = c.String(),
                        Detail = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Metrics");
        }
    }
}
