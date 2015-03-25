namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveOptionIdFromVote : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Votes", "OptionId", "dbo.Options");
            DropIndex("dbo.Votes", new[] { "OptionId" });
            RenameColumn(table: "dbo.Votes", name: "OptionId", newName: "Option_Id");
            AlterColumn("dbo.Votes", "Option_Id", c => c.Long());
            CreateIndex("dbo.Votes", "Option_Id");
            AddForeignKey("dbo.Votes", "Option_Id", "dbo.Options", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Votes", "Option_Id", "dbo.Options");
            DropIndex("dbo.Votes", new[] { "Option_Id" });
            AlterColumn("dbo.Votes", "Option_Id", c => c.Long(nullable: false));
            RenameColumn(table: "dbo.Votes", name: "Option_Id", newName: "OptionId");
            CreateIndex("dbo.Votes", "OptionId");
            AddForeignKey("dbo.Votes", "OptionId", "dbo.Options", "Id", cascadeDelete: true);
        }
    }
}
