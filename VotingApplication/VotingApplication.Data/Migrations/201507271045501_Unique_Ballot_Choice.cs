namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Unique_Ballot_Choice : DbMigration
    {
        public override void Up()
        {
            Sql(@"
                set nocount on;
                WITH grouping AS (
                    SELECT
                        v.Id,
                        row_number() over (PARTITION BY v.Ballot_Id, v.Choice_Id ORDER BY v.Choice_Id) AS [num]
                    FROM 
                        Votes v
                )

                DELETE v from Votes v JOIN grouping g ON g.Id = v.Id
                WHERE g.num > 1
            ");
            CreateIndex("Votes", new string[2] { "Choice_Id", "Ballot_Id" }, true, "IX_ChoiceBallot");
        }

        public override void Down()
        {
            DropIndex("Votes", "IX_ChoiceBallot");
        }
    }
}
