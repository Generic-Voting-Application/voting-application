namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class PollChoiceNumberTrigger : DbMigration
    {
        public override void Up()
        {
            // Make sure the default value is value wrt the check constraint below.
            AlterColumn("dbo.Choices", "PollChoiceNumber", c => c.Int(nullable: false, defaultValue: 1));

            // Initially set all the values to ensure no zero values.
            Sql(@"with choice_cte as (
                    select 
                        c.Id as ChoiceId,
                        row_number() over (partition by c.Poll_Id order by c.Poll_Id) as ChoiceNumber
                    from Choices c
                )

                update c
                set 
                    c.pollChoiceNumber = cte.ChoiceNumber
                from Choices c
                join choice_cte cte on cte.ChoiceId = c.Id");

            // Add a check constraint
            Sql(@"alter table Choices with check add constraint CK_Choices_PollChoiceNumber_GreaterThanZero
                    check (PollChoiceNumber > 0)");

            // Add a trigger to update when values inserted (add or update).
            Sql(@"create trigger TR_Choices_insert_update
                    on dbo.Choices
                    after insert, update
                    as

                        set nocount on;
                        with choice_cte as (
                            select 
                                c.Id as ChoiceId,
                                row_number() over (partition by c.Poll_Id order by c.Poll_Id) as pollChoiceNumber
                            from Choices c
                            join inserted i on i.Poll_Id = c.Poll_Id
                        )

                        update c
                        set 
                            c.pollChoiceNumber = cte.pollChoiceNumber
                        from Choices c
                        join choice_cte cte on cte.ChoiceId = c.Id
                        join inserted i on i.Poll_Id = c.Poll_Id");

            // Add a trigger to update when values deleted.
            Sql(@"create trigger TR_Choices_delete
                    on dbo.Choices
                    after delete
                    as

                        set nocount on;
                        with choice_cte as (
                            select 
                                c.Id as ChoiceId,
                                row_number() over (partition by c.Poll_Id order by c.Poll_Id) as pollChoiceNumber
                            from Choices c
                            join deleted d on d.Poll_Id = c.Poll_Id
                        )

                        update c
                        set 
                            c.pollChoiceNumber = cte.pollChoiceNumber
                        from Choices c
                        join choice_cte cte on cte.ChoiceId = c.Id
                        join deleted d on d.Poll_Id = c.Poll_Id");
        }

        public override void Down()
        {
            AlterColumn("dbo.Choices", "PollChoiceNumber", c => c.Int(nullable: false));

            Sql(@"alter table Choices drop constraint CK_Choices_PollChoiceNumber_GreaterThanZero");
            Sql(@"drop trigger TR_Choices_insert_update");
            Sql(@"drop trigger TR_Choices_delete");
        }
    }
}
