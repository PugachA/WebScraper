using Microsoft.EntityFrameworkCore.Migrations;

namespace WebScraper.Data.Migrations
{
    public partial class AddHtmlLoaderColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HtmlLoader",
                table: "SiteSettings",
                maxLength: 50,
                nullable: false,
                defaultValue: "HttpLoader");

            migrationBuilder.Sql(@"
UPDATE dbo.SiteSettings
SET HtmlLoader = N'SeleniumLoader'
WHERE UseSeleniumService = 1");

            migrationBuilder.DropColumn(
                name: "UseSeleniumService",
                table: "SiteSettings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseSeleniumService",
                table: "SiteSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
UPDATE dbo.SiteSettings
SET UseSeleniumService = 1
WHERE HtmlLoader = N'SeleniumLoader'");

            migrationBuilder.DropColumn(
                name: "HtmlLoader",
                table: "SiteSettings");
        }
    }
}
