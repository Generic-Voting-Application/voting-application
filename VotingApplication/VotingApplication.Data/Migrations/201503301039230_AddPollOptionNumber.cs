namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddPollOptionNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Options", "PollOptionNumber", c => c.Int(nullable: false));
            Sql(@"with option_cte as (
                    select 
                        o.Id as OptionId,
                        row_number() over (partition by o.Poll_Id order by o.Poll_Id) as pollOptionNumber
                    from Options o
                )

                update o
                set 
                    o.pollOptionNumber = cte.pollOptionNumber
                from Options o
                join option_cte cte on cte.OptionId = o.Id");
        }

        public override void Down()
        {
            DropColumn("dbo.Options", "PollOptionNumber");
        }
    }
}
