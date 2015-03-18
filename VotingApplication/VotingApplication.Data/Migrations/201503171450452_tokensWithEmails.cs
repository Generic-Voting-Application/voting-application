namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tokensWithEmails : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Votes", "Token_Id", "dbo.Tokens");
            DropIndex("dbo.Votes", new[] { "Token_Id" });
            RenameColumn(table: "dbo.Votes", name: "Token_Id", newName: "TokenId");
            AddColumn("dbo.Tokens", "Email", c => c.String());
            AlterColumn("dbo.Votes", "TokenId", c => c.Long(nullable: false));
            CreateIndex("dbo.Votes", "TokenId");
            AddForeignKey("dbo.Votes", "TokenId", "dbo.Tokens", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Votes", "TokenId", "dbo.Tokens");
            DropIndex("dbo.Votes", new[] { "TokenId" });
            AlterColumn("dbo.Votes", "TokenId", c => c.Long());
            DropColumn("dbo.Tokens", "Email");
            RenameColumn(table: "dbo.Votes", name: "TokenId", newName: "Token_Id");
            CreateIndex("dbo.Votes", "Token_Id");
            AddForeignKey("dbo.Votes", "Token_Id", "dbo.Tokens", "Id");
        }
    }
}
