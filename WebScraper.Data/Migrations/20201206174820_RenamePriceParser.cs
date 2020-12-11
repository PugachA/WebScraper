using Microsoft.EntityFrameworkCore.Migrations;

namespace WebScraper.Data.Migrations
{
    public partial class RenamePriceParser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PriceParser",
                table: "SiteSettings",
                maxLength: 50,
                nullable: false,
                defaultValue: "HtmlPriceParser",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "PriceParser",
                keyValue: "PriceParser",
                columns: new[] { "PriceParser" },
                values: new object[] { "HtmlPriceParser" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PriceParser",
                table: "SiteSettings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldDefaultValue: "HtmlPriceParser");

            migrationBuilder.UpdateData(
                table: "SiteSettings",
                keyColumn: "PriceParser",
                keyValue: "HtmlPriceParser",
                columns: new[] { "PriceParser" },
                values: new object[] { "PriceParser" });
        }
    }
}
