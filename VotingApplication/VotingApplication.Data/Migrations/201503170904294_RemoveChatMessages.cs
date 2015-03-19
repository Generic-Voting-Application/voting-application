namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveChatMessages : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ChatMessages", "Poll_Id", "dbo.Polls");
            DropIndex("dbo.ChatMessages", new[] { "Poll_Id" });
            DropTable("dbo.ChatMessages");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        VoterName = c.String(),
                        Message = c.String(),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        Poll_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.ChatMessages", "Poll_Id");
            AddForeignKey("dbo.ChatMessages", "Poll_Id", "dbo.Polls", "Id");
        }
    }
}
