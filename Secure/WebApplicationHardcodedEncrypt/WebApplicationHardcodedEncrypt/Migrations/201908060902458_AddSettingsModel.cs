namespace WebApplicationHardcodedEncrypt.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSettingsModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SettingsModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Key = c.String(),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SettingsModels");
        }
    }
}
