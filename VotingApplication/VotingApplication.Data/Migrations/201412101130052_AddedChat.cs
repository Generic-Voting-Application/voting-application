namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedChat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Message = c.String(),
                        User_Id = c.Long(),
                        Poll_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .ForeignKey("dbo.Polls", t => t.Poll_Id)
                .Index(t => t.User_Id)
                .Index(t => t.Poll_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChatMessages", "Poll_Id", "dbo.Polls");
            DropForeignKey("dbo.ChatMessages", "User_Id", "dbo.Users");
            DropIndex("dbo.ChatMessages", new[] { "Poll_Id" });
            DropIndex("dbo.ChatMessages", new[] { "User_Id" });
            DropTable("dbo.ChatMessages");
        }
    }
}
