namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class LastUpdated_CreatedDate_Utc : DbMigration
    {
        public override void Up()
        {
            /* This could be done with a simple change to use sp_rename by
             * calling RenameColumn(), but that
             * a) doesn't rename the constraints
             * b) doesn't allow us to change the default constraint value
             */

            AddColumn("dbo.Polls", "LastUpdatedUtc", c => c.DateTime(nullable: false));
            AddColumn("dbo.Polls", "CreatedDateUtc", c => c.DateTime(nullable: false));

            // Fix default constraint names and values for new columns
            Sql(@" 
                    declare @CreatedDateUtcConstraint nvarchar(128);
                    
                    select @CreatedDateUtcConstraint = name
                    from sys.default_constraints
                    where parent_object_id = object_id(N'dbo.Polls')
                    and col_name(parent_object_id, parent_column_id) = 'CreatedDateUtc';

                    if @CreatedDateUtcConstraint is not null
                        execute('alter table dbo.Polls drop constraint ' + @CreatedDateUtcConstraint);
                   
                    alter table dbo.Polls
                    add constraint DF_Polls_CreatedDateUtc default getutcdate() for CreatedDateUtc;


                    declare @lastUpdatedUtcConstraint nvarchar(128);
                    
                    select @lastUpdatedUtcConstraint = name
                    from sys.default_constraints
                    where parent_object_id = object_id(N'dbo.Polls')
                    and col_name(parent_object_id, parent_column_id) = 'LastUpdatedUtc';

                    if @lastUpdatedUtcConstraint is not null
                        execute('alter table dbo.Polls drop constraint ' + @lastUpdatedUtcConstraint);

                    alter table dbo.Polls
                    add constraint DF_Polls_LastUpdatedUtc default getutcdate() for LastUpdatedUtc;
                ");

            // Migrate the date to the new column name
            Sql(@"
                    update Polls
                    set
                        LastUpdatedUtc = LastUpdated,
                        CreatedDateUtc = CreatedDate
                ");

            DropColumn("dbo.Polls", "LastUpdated");
            DropColumn("dbo.Polls", "CreatedDate");
        }

        public override void Down()
        {
            AddColumn("dbo.Polls", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Polls", "LastUpdated", c => c.DateTime(nullable: false));

            // Migrate the date to the new column name
            Sql(@"
                    update Polls
                    set
                        LastUpdated = LastUpdatedUtc,
                        CreatedDate = CreatedDateUtc
                ");

            DropColumn("dbo.Polls", "CreatedDateUtc");
            DropColumn("dbo.Polls", "LastUpdatedUtc");
        }
    }
}
