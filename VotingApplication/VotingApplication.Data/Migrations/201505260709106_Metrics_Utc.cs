namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Metrics_Utc : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Metrics", "Timestamp", "TimestampUtc");
            RenameIndex("dbo.Metrics", "IX_Timestamp", "IX_TimestampUtc");


            // Rename default constraint.
            Sql(@"
                    declare @TimeStampUtcConstraint nvarchar(128);

                    select @TimeStampUtcConstraint = name
                    from sys.default_constraints
                    where parent_object_id = object_id(N'dbo.Metrics')
                    and col_name(parent_object_id, parent_column_id) = 'TimestampUtc';

                    if @TimeStampUtcConstraint is not null
                        execute('alter table dbo.Metrics drop constraint ' + @TimeStampUtcConstraint);

                    alter table dbo.Metrics
                    add constraint DF_Metrics_TimestampUtc default getutcdate() for TimestampUtc;
                ");
        }

        public override void Down()
        {
            RenameColumn("dbo.Metrics", "TimestampUtc", "Timestamp");
            RenameIndex("dbo.Metrics", "IX_TimestampUtc", "IX_Timestamp");

            /* Rename default constraint
             * Note that this won't actually put things back exactly as they 
             * were, as the original constraint was created with a default name
             * by Sql Server, but we're now setting a more deterministic one.
             */
            Sql(@"exec sp_rename @objname = N'DF_Metrics_TimestampUtc', @newname = N'DF_Metrics_Timestamp', @objtype = N'OBJECT'");
        }
    }
}
