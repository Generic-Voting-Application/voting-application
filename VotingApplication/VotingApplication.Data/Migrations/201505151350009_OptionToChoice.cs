namespace VotingApplication.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class OptionToChoice : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Votes", "Option_Id", "dbo.Options");
            DropForeignKey("dbo.Options", "Poll_Id", "dbo.Polls");
            DropIndex("dbo.Votes", new[] { "Option_Id" });
            DropIndex("dbo.Options", new[] { "Poll_Id" });
            CreateTable(
                "dbo.Choices",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        PollChoiceNumber = c.Int(nullable: false),
                        Poll_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Polls", t => t.Poll_Id)
                .Index(t => t.Poll_Id);

            AddColumn("dbo.Votes", "Choice_Id", c => c.Long());
            AddColumn("dbo.Polls", "ChoiceAdding", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Votes", "Choice_Id");
            AddForeignKey("dbo.Votes", "Choice_Id", "dbo.Choices", "Id");

            Sql(@"INSERT INTO dbo.Choices (Name, Description, PollChoiceNumber, Poll_Id)
                 SELECT Name, Description, PollOptionNumber, Poll_Id
                 FROM dbo.Options");

            Sql(@"UPDATE dbo.Polls
                  SET ChoiceAdding = OptionAdding");

            Sql(@"UPDATE dbo.Votes
                  SET Choice_Id = Option_Id");

            DropColumn("dbo.Votes", "Option_Id");
            DropColumn("dbo.Polls", "OptionAdding");
            DropTable("dbo.Options");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.Options",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        PollOptionNumber = c.Int(nullable: false),
                        Poll_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.Polls", "OptionAdding", c => c.Boolean(nullable: false));
            AddColumn("dbo.Votes", "Option_Id", c => c.Long());
            DropForeignKey("dbo.Choices", "Poll_Id", "dbo.Polls");
            DropForeignKey("dbo.Votes", "Choice_Id", "dbo.Choices");
            DropIndex("dbo.Choices", new[] { "Poll_Id" });
            DropIndex("dbo.Votes", new[] { "Choice_Id" });

            CreateIndex("dbo.Options", "Poll_Id");
            CreateIndex("dbo.Votes", "Option_Id");
            AddForeignKey("dbo.Options", "Poll_Id", "dbo.Polls", "Id");
            AddForeignKey("dbo.Votes", "Option_Id", "dbo.Options", "Id");

            Sql(@"UPDATE dbo.Polls
                  SET OptionAdding = ChoiceAdding");

            Sql(@"UPDATE dbo.Votes
                  SET Option_Id = Choice_Id");

            Sql(@"INSERT INTO dbo.Options (Name, Description, PollOptionNumber, Poll_Id)
                 SELECT Name, Description, PollChoiceNumber, Poll_Id
                 FROM dbo.Choices");

            DropColumn("dbo.Polls", "ChoiceAdding");
            DropColumn("dbo.Votes", "Choice_Id");
            DropTable("dbo.Choices");
        }
    }
}
