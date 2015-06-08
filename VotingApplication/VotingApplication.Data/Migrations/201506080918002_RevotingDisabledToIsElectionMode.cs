namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RevotingDisabledToIsElectionMode : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Polls", "RevotingDisabled", "IsElectionMode");
        }

        public override void Down()
        {
            RenameColumn("dbo.Polls", "IsElectionMode", "RevotingDisabled");
        }
    }
}
