using Microsoft.EntityFrameworkCore.Migrations;

namespace WebScraper.WebApi.Migrations
{
    public partial class AddNameAttribute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Prices",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Prices");
        }
    }
}
