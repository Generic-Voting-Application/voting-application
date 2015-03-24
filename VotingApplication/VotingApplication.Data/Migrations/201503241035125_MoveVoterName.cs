namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MoveVoterName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tokens", "VoterName", c => c.String());

            Sql(@"with ordered_cte as (
                    select 
                        t.TokenGuid,
                        v.VoterName,
                        row_number() over (partition by t.TokenGuid, v.VoterName order by v.Id desc) as inserted_order
                    from Votes v
                    join Tokens t on t.Id = v.Token_Id
                )

                update t
                set
                    t.VoterName = cte.VoterName
                from Tokens t
                join ordered_cte cte on cte.TokenGuid = t.TokenGuid
                where inserted_order = 1");

            DropColumn("dbo.Votes", "VoterName");
        }

        public override void Down()
        {
            AddColumn("dbo.Votes", "VoterName", c => c.String());
            DropColumn("dbo.Tokens", "VoterName");
        }
    }
}
