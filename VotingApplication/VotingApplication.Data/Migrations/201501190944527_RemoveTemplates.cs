namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RemoveTemplates : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PollOptions", "Poll_Id", "dbo.Polls");
            DropForeignKey("dbo.PollOptions", "Option_Id", "dbo.Options");
            DropForeignKey("dbo.TemplateOptions", "Template_Id", "dbo.Templates");
            DropForeignKey("dbo.TemplateOptions", "Option_Id", "dbo.Options");
            DropIndex("dbo.PollOptions", new[] { "Poll_Id" });
            DropIndex("dbo.PollOptions", new[] { "Option_Id" });
            DropIndex("dbo.TemplateOptions", new[] { "Template_Id" });
            DropIndex("dbo.TemplateOptions", new[] { "Option_Id" });
            AddColumn("dbo.Options", "Poll_Id", c => c.Long());
            AddColumn("dbo.Polls", "CreatorIdentity", c => c.String());
            AddColumn("dbo.Polls", "AnonymousVoting", c => c.Boolean(nullable: false));
            AddColumn("dbo.Polls", "RequireAuth", c => c.Boolean(nullable: false));
            AddColumn("dbo.Polls", "Expires", c => c.Boolean(nullable: false));
            AddColumn("dbo.Polls", "ExpiryDate", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Polls", "OptionAdding", c => c.Boolean(nullable: false));
            AddColumn("dbo.Polls", "LastUpdated", c => c.DateTime(nullable: false));
            AddColumn("dbo.Polls", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.ChatMessages", "Timestamp", c => c.DateTimeOffset(nullable: false, precision: 7));
            AddColumn("dbo.Users", "PollId", c => c.Guid(nullable: false));
            AddColumn("dbo.Users", "Token_Id", c => c.Long());
            AddColumn("dbo.Tokens", "UserId", c => c.Long(nullable: false));
            AddColumn("dbo.Tokens", "PollId", c => c.Guid(nullable: false));
            CreateIndex("dbo.Options", "Poll_Id");
            CreateIndex("dbo.Users", "Token_Id");
            AddForeignKey("dbo.Users", "Token_Id", "dbo.Tokens", "Id");
            AddForeignKey("dbo.Options", "Poll_Id", "dbo.Polls", "Id");
            DropColumn("dbo.Polls", "TemplateId");
            DropTable("dbo.Templates");
            DropTable("dbo.PollOptions");
            DropTable("dbo.TemplateOptions");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.TemplateOptions",
                c => new
                {
                    Template_Id = c.Long(nullable: false),
                    Option_Id = c.Long(nullable: false),
                })
                .PrimaryKey(t => new { t.Template_Id, t.Option_Id });

            CreateTable(
                "dbo.PollOptions",
                c => new
                {
                    Poll_Id = c.Long(nullable: false),
                    Option_Id = c.Long(nullable: false),
                })
                .PrimaryKey(t => new { t.Poll_Id, t.Option_Id });

            CreateTable(
                "dbo.Templates",
                c => new
                {
                    Id = c.Long(nullable: false, identity: true),
                    Name = c.String(),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Polls", "TemplateId", c => c.Long(nullable: false));
            DropForeignKey("dbo.Options", "Poll_Id", "dbo.Polls");
            DropForeignKey("dbo.Users", "Token_Id", "dbo.Tokens");
            DropIndex("dbo.Users", new[] { "Token_Id" });
            DropIndex("dbo.Options", new[] { "Poll_Id" });
            DropColumn("dbo.Tokens", "PollId");
            DropColumn("dbo.Tokens", "UserId");
            DropColumn("dbo.Users", "Token_Id");
            DropColumn("dbo.Users", "PollId");
            DropColumn("dbo.ChatMessages", "Timestamp");
            DropColumn("dbo.Polls", "CreatedDate");
            DropColumn("dbo.Polls", "LastUpdated");
            DropColumn("dbo.Polls", "OptionAdding");
            DropColumn("dbo.Polls", "ExpiryDate");
            DropColumn("dbo.Polls", "Expires");
            DropColumn("dbo.Polls", "RequireAuth");
            DropColumn("dbo.Polls", "AnonymousVoting");
            DropColumn("dbo.Polls", "CreatorIdentity");
            DropColumn("dbo.Options", "Poll_Id");
            CreateIndex("dbo.TemplateOptions", "Option_Id");
            CreateIndex("dbo.TemplateOptions", "Template_Id");
            CreateIndex("dbo.PollOptions", "Option_Id");
            CreateIndex("dbo.PollOptions", "Poll_Id");
            AddForeignKey("dbo.TemplateOptions", "Option_Id", "dbo.Options", "Id", cascadeDelete: true);
            AddForeignKey("dbo.TemplateOptions", "Template_Id", "dbo.Templates", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PollOptions", "Option_Id", "dbo.Options", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PollOptions", "Poll_Id", "dbo.Polls", "Id", cascadeDelete: true);
        }
    }
}
