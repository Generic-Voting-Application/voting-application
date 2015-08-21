namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddHiddenResults : DbMigration
    {
        public override void Up()
        {
            // Drop the column if there's been an automatic migration to add it.
            // We want to explicitly add the migration.
            // Unfortunately this does mean there might still be an auto migration
            // in the history table, but we don't roll back, so we should be ok.
            Sql(@"  -- Remove any default constraints
                    declare @constraintName nvarchar(128);

                    select @constraintName = name
                    from sys.default_constraints
                    where parent_object_id = object_id(N'dbo.Polls')
                    and col_name(parent_object_id, parent_column_id) = 'HiddenResults';

                    if @constraintName is not null
                    begin
                        execute('alter table [dbo].[Polls] drop constraint [' + @constraintName + ']')
                    end

                    -- Remove the column
                    if exists 
                        (select * from INFORMATION_SCHEMA.COLUMNS
                        where TABLE_NAME = 'Polls'
                        and COLUMN_NAME = 'HiddenResults')
                    begin
                        alter table dbo.Polls
                        drop column HiddenResults
                    end");

            AddColumn("dbo.Polls", "HiddenResults", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Polls", "HiddenResults");


        }
    }
}
