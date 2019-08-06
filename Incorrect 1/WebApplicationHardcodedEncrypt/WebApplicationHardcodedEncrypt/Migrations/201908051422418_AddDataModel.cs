namespace WebApplicationHardcodedEncrypt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDataModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(),
                        EncodingText = c.Binary(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DataModels");
        }
    }
}
