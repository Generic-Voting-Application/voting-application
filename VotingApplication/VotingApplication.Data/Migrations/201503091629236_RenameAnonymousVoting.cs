namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameAnonymousVoting : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Users", "Token_Id", "dbo.Tokens");
            DropForeignKey("dbo.ChatMessages", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Votes", "UserId", "dbo.Users");
            DropIndex("dbo.ChatMessages", new[] { "User_Id" });
            DropIndex("dbo.Users", new[] { "Token_Id" });
            DropIndex("dbo.Votes", new[] { "UserId" });
            AddColumn("dbo.Polls", "PollType", c => c.Int(nullable: false));
            AddColumn("dbo.Polls", "NamedVoting", c => c.Boolean(nullable: false));
            AddColumn("dbo.ChatMessages", "VoterName", c => c.String());
            AddColumn("dbo.Votes", "VoteValue", c => c.Int(nullable: false));
            AddColumn("dbo.Votes", "VoterName", c => c.String());
            DropColumn("dbo.Polls", "VotingStrategy");
            DropColumn("dbo.Polls", "AnonymousVoting");
            DropColumn("dbo.ChatMessages", "User_Id");
            DropColumn("dbo.Tokens", "UserId");
            DropColumn("dbo.Tokens", "PollId");
            DropColumn("dbo.Votes", "PollValue");
            DropColumn("dbo.Votes", "UserId");
            DropTable("dbo.Users");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        PollId = c.Guid(nullable: false),
                        Token_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Votes", "UserId", c => c.Long(nullable: false));
            AddColumn("dbo.Votes", "PollValue", c => c.Int(nullable: false));
            AddColumn("dbo.Tokens", "PollId", c => c.Guid(nullable: false));
            AddColumn("dbo.Tokens", "UserId", c => c.Long(nullable: false));
            AddColumn("dbo.ChatMessages", "User_Id", c => c.Long());
            AddColumn("dbo.Polls", "AnonymousVoting", c => c.Boolean(nullable: false));
            AddColumn("dbo.Polls", "VotingStrategy", c => c.String());
            DropColumn("dbo.Votes", "VoterName");
            DropColumn("dbo.Votes", "VoteValue");
            DropColumn("dbo.ChatMessages", "VoterName");
            DropColumn("dbo.Polls", "NamedVoting");
            DropColumn("dbo.Polls", "PollType");
            CreateIndex("dbo.Votes", "UserId");
            CreateIndex("dbo.Users", "Token_Id");
            CreateIndex("dbo.ChatMessages", "User_Id");
            AddForeignKey("dbo.Votes", "UserId", "dbo.Users", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ChatMessages", "User_Id", "dbo.Users", "Id");
            AddForeignKey("dbo.Users", "Token_Id", "dbo.Tokens", "Id");
        }
    }
}
