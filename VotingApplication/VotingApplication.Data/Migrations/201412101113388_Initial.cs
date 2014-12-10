namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Options",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Info = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Polls",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        UUID = c.Guid(nullable: false),
                        ManageID = c.Guid(nullable: false),
                        Name = c.String(),
                        Creator = c.String(),
                        VotingStrategy = c.String(),
                        TemplateId = c.Long(nullable: false),
                        MaxPoints = c.Int(nullable: false),
                        MaxPerVote = c.Int(nullable: false),
                        InviteOnly = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Tokens",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        TokenGuid = c.Guid(nullable: false),
                        Poll_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Polls", t => t.Poll_Id)
                .Index(t => t.Poll_Id);
            
            CreateTable(
                "dbo.Templates",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Votes",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OptionId = c.Long(nullable: false),
                        PollValue = c.Int(nullable: false),
                        UserId = c.Long(nullable: false),
                        PollId = c.Guid(nullable: false),
                        Poll_Id = c.Long(),
                        Token_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Options", t => t.OptionId, cascadeDelete: true)
                .ForeignKey("dbo.Polls", t => t.Poll_Id)
                .ForeignKey("dbo.Tokens", t => t.Token_Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.OptionId)
                .Index(t => t.UserId)
                .Index(t => t.Poll_Id)
                .Index(t => t.Token_Id);
            
            CreateTable(
                "dbo.PollOptions",
                c => new
                    {
                        Poll_Id = c.Long(nullable: false),
                        Option_Id = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.Poll_Id, t.Option_Id })
                .ForeignKey("dbo.Polls", t => t.Poll_Id, cascadeDelete: true)
                .ForeignKey("dbo.Options", t => t.Option_Id, cascadeDelete: true)
                .Index(t => t.Poll_Id)
                .Index(t => t.Option_Id);
            
            CreateTable(
                "dbo.TemplateOptions",
                c => new
                    {
                        Template_Id = c.Long(nullable: false),
                        Option_Id = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.Template_Id, t.Option_Id })
                .ForeignKey("dbo.Templates", t => t.Template_Id, cascadeDelete: true)
                .ForeignKey("dbo.Options", t => t.Option_Id, cascadeDelete: true)
                .Index(t => t.Template_Id)
                .Index(t => t.Option_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Votes", "UserId", "dbo.Users");
            DropForeignKey("dbo.Votes", "Token_Id", "dbo.Tokens");
            DropForeignKey("dbo.Votes", "Poll_Id", "dbo.Polls");
            DropForeignKey("dbo.Votes", "OptionId", "dbo.Options");
            DropForeignKey("dbo.TemplateOptions", "Option_Id", "dbo.Options");
            DropForeignKey("dbo.TemplateOptions", "Template_Id", "dbo.Templates");
            DropForeignKey("dbo.Tokens", "Poll_Id", "dbo.Polls");
            DropForeignKey("dbo.PollOptions", "Option_Id", "dbo.Options");
            DropForeignKey("dbo.PollOptions", "Poll_Id", "dbo.Polls");
            DropIndex("dbo.TemplateOptions", new[] { "Option_Id" });
            DropIndex("dbo.TemplateOptions", new[] { "Template_Id" });
            DropIndex("dbo.PollOptions", new[] { "Option_Id" });
            DropIndex("dbo.PollOptions", new[] { "Poll_Id" });
            DropIndex("dbo.Votes", new[] { "Token_Id" });
            DropIndex("dbo.Votes", new[] { "Poll_Id" });
            DropIndex("dbo.Votes", new[] { "UserId" });
            DropIndex("dbo.Votes", new[] { "OptionId" });
            DropIndex("dbo.Tokens", new[] { "Poll_Id" });
            DropTable("dbo.TemplateOptions");
            DropTable("dbo.PollOptions");
            DropTable("dbo.Votes");
            DropTable("dbo.Users");
            DropTable("dbo.Templates");
            DropTable("dbo.Tokens");
            DropTable("dbo.Polls");
            DropTable("dbo.Options");
        }
    }
}
