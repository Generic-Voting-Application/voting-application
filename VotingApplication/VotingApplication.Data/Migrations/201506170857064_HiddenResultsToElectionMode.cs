namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class HiddenResultsToElectionMode : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Polls", "HiddenResults", "ElectionMode");
        }

        public override void Down()
        {
            RenameColumn("dbo.Polls", "ElectionMode", "HiddenResults");
        }
    }
}
