namespace VotingApplication.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameTokenToBallot : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Tokens", newName: "Ballots");
            RenameColumn(table: "dbo.Votes", name: "Token_Id", newName: "Ballot_Id");
            RenameIndex(table: "dbo.Votes", name: "IX_Token_Id", newName: "IX_Ballot_Id");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Votes", name: "IX_Ballot_Id", newName: "IX_Token_Id");
            RenameColumn(table: "dbo.Votes", name: "Ballot_Id", newName: "Token_Id");
            RenameTable(name: "dbo.Ballots", newName: "Tokens");
        }
    }
}
