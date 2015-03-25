namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTokenIdFromVoteObject : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Votes", "TokenId", "dbo.Tokens");
            DropIndex("dbo.Votes", new[] { "TokenId" });
            RenameColumn(table: "dbo.Votes", name: "TokenId", newName: "Token_Id");
            AlterColumn("dbo.Votes", "Token_Id", c => c.Long());
            CreateIndex("dbo.Votes", "Token_Id");
            AddForeignKey("dbo.Votes", "Token_Id", "dbo.Tokens", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Votes", "Token_Id", "dbo.Tokens");
            DropIndex("dbo.Votes", new[] { "Token_Id" });
            AlterColumn("dbo.Votes", "Token_Id", c => c.Long(nullable: false));
            RenameColumn(table: "dbo.Votes", name: "Token_Id", newName: "TokenId");
            CreateIndex("dbo.Votes", "TokenId");
            AddForeignKey("dbo.Votes", "TokenId", "dbo.Tokens", "Id", cascadeDelete: true);
        }
    }
}
