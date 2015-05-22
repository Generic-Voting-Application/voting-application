namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ExpiryDate_Utc : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Polls", "ExpiryDateUtc", c => c.DateTime());

            /* Migrate existing data
             *  This is destructive, as we lose the time offset.
             *  However, we're being consistent in saving the expiry date as
             *  UTC, and whatever offset the user is we'll calculate when we
             *  need to display it.
             */
            Sql(@"
                    update Polls
                    set ExpiryDateUtc = ExpiryDate
                ");

            DropColumn("dbo.Polls", "ExpiryDate");
        }

        public override void Down()
        {
            AddColumn("dbo.Polls", "ExpiryDate", c => c.DateTimeOffset(precision: 7));

            // Migrate existing data
            Sql(@"
                    update Polls
                    set ExpiryDate = ExpiryDateUtc
                ");

            DropColumn("dbo.Polls", "ExpiryDateUtc");
        }
    }
}
