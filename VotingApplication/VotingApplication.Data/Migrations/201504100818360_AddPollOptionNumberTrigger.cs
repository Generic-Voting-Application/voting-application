namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddPollOptionNumberTrigger : DbMigration
    {
        public override void Up()
        {
            // Make sure the default value is value wrt the check constraint below.
            AlterColumn("dbo.Options", "PollOptionNumber", c => c.Int(nullable: false, defaultValue: 1));

            // Initially set all the values to ensure no zero values.
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

            // Add a check constraint
            Sql(@"alter table Options with check add constraint CK_Options_PollOptionNumber_GreaterThanZero
                    check (PollOptionNumber > 0)");

            // Add a trigger to update when values inserted (add or update).
            Sql(@"create trigger TR_Options_insert_update
                    on dbo.Options
                    after insert, update
                    as

                        set nocount on;
                        with option_cte as (
                            select 
                                o.Id as OptionId,
                                row_number() over (partition by o.Poll_Id order by o.Poll_Id) as pollOptionNumber
                            from Options o
                            join inserted i on i.Poll_Id = o.Poll_Id
                        )

                        update o
                        set 
                            o.pollOptionNumber = cte.pollOptionNumber
                        from Options o
                        join option_cte cte on cte.OptionId = o.Id
                        join inserted i on i.Poll_Id = o.Poll_Id");

            // Add a trigger to update when values deleted.
            Sql(@"create trigger TR_Options_delete
                    on dbo.Options
                    after delete
                    as

                        set nocount on;
                        with option_cte as (
                            select 
                                o.Id as OptionId,
                                row_number() over (partition by o.Poll_Id order by o.Poll_Id) as pollOptionNumber
                            from Options o
                            join deleted d on d.Poll_Id = o.Poll_Id
                        )

                        update o
                        set 
                            o.pollOptionNumber = cte.pollOptionNumber
                        from Options o
                        join option_cte cte on cte.OptionId = o.Id
                        join deleted d on d.Poll_Id = o.Poll_Id");
        }

        public override void Down()
        {
            AlterColumn("dbo.Options", "PollOptionNumber", c => c.Int(nullable: false));

            Sql(@"alter table Options drop constraint CK_Options_PollOptionNumber_GreaterThanZero");
            Sql(@"drop trigger TR_Options_insert_update");
            Sql(@"drop trigger TR_Options_delete");
        }
    }
}
