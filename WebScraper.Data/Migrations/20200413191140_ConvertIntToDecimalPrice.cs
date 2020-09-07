using Microsoft.EntityFrameworkCore.Migrations;

namespace WebScraper.Data.Migrations
{
    public partial class ConvertIntToDecimalPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Prices",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "DicountPrice",
                table: "Prices",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.Sql(
                @"UPDATE [ProductWatcher].[dbo].[Prices]
                  SET 
                    Price = Price / 100,
                    DicountPrice = DicountPrice / 100");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Price",
                table: "Prices",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<int>(
                name: "DicountPrice",
                table: "Prices",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.Sql(
                @"UPDATE [ProductWatcher].[dbo].[Prices]
                  SET 
                    Price = Price * 100,
                    DicountPrice = DicountPrice * 100");
        }
    }
}
