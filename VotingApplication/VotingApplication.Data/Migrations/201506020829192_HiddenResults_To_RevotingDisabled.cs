namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class HiddenResults_To_RevotingDisabled : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Polls", "HiddenResults", "RevotingDisabled");
        }

        public override void Down()
        {
            RenameColumn("dbo.Polls", "RevotingDisabled", "HiddenResults");
        }
    }
}
