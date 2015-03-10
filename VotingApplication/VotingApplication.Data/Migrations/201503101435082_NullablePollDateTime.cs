namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NullablePollDateTime : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Polls", "ExpiryDate", c => c.DateTimeOffset(precision: 7));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Polls", "ExpiryDate", c => c.DateTimeOffset(nullable: false, precision: 7));
        }
    }
}
