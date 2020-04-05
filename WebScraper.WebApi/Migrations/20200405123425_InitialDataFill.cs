using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

namespace WebScraper.WebApi.Migrations
{
    public partial class InitialDataFill : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string initialSql = File.ReadAllText("InitialDataFill.sql");
            migrationBuilder.Sql(initialSql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
