namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddManageGuidToBallot : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Ballots", "ManageGuid", c => c.Guid(nullable: false, defaultValueSql: "newid()"));
        }

        public override void Down()
        {
            DropColumn("dbo.Ballots", "ManageGuid");
        }
    }
}
