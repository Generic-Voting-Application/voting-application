namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RemovePollOptionNumberCheckConstraint : DbMigration
    {
        public override void Up()
        {
            Sql(@"alter table Options drop constraint CK_Options_PollOptionNumber_GreaterThanZero");
        }

        public override void Down()
        {
        }
    }
}
