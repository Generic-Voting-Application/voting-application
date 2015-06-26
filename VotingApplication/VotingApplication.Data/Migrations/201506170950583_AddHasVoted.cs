namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddHasVoted : DbMigration
    {
        public override void Up()
        {
            Sql("ALTER TABLE [dbo].[Ballots] ADD [HasVoted] [bit] CONSTRAINT DF_BALLOTS_HASVOTED default 0 NOT NULL");
        }

        public override void Down()
        {
            DropColumn("dbo.Ballots", "HasVoted");
        }
    }
}
