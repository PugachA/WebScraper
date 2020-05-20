using Microsoft.EntityFrameworkCore.Migrations;

namespace WebScraper.WebApi.Migrations
{
    public partial class ReplaceBaseUrlToSite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseUrl",
                table: "SiteSettings");

            migrationBuilder.AddColumn<string>(
                name: "BaseUrl",
                table: "Sites",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseUrl",
                table: "Sites");

            migrationBuilder.AddColumn<string>(
                name: "BaseUrl",
                table: "SiteSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
