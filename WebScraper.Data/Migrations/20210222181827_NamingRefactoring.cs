using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebScraper.Data.Migrations
{
    public partial class NamingRefactoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Products",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    DiscountPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    DiscountPercentage = table.Column<double>(nullable: true, computedColumnSql: "CONVERT(DECIMAL(18, 2), 100*([Price]-[DiscountPrice])/[Price]"),
                    AdditionalInformation = table.Column<string>(maxLength: 1024, nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    ProductId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductData_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductData_ProductId",
                table: "ProductData",
                column: "ProductId");

            migrationBuilder.Sql(@"
INSERT INTO dbo.ProductData (Name, Price, DiscountPrice, AdditionalInformation, Date, ProductId)
SELECT Name, Price, DicountPrice, AdditionalInformation, Date, ProductId FROM dbo.Prices");

            migrationBuilder.DropTable(
                name: "Prices");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Products");

            migrationBuilder.CreateTable(
                name: "Prices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdditionalInformation = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DicountPrice = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    DiscountPercentage = table.Column<double>(type: "float", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prices_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prices_ProductId",
                table: "Prices",
                column: "ProductId");

            migrationBuilder.Sql(@"
INSERT INTO dbo.Prices (Name, Price, DicountPrice, DiscountPercentage, AdditionalInformation, Date, ProductId)
SELECT Name, Price, DiscountPrice, DiscountPercentage, AdditionalInformation, Date, ProductId FROM dbo.ProductData");

            migrationBuilder.DropTable(
                name: "ProductData");
        }
    }
}
