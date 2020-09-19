using Microsoft.EntityFrameworkCore.Migrations;

namespace WebScraper.Data.Migrations
{
    public partial class InitialDataSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_SiteSettings_SettingsId",
                table: "Sites");

            migrationBuilder.AlterColumn<int>(
                name: "SettingsId",
                table: "Sites",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "SiteSettings",
                columns: new[] { "Id", "AutoGenerateSchedule", "CheckInterval", "MinCheckInterval", "UseSeleniumService" },
                values: new object[] { 1, false, "00:01:00", "00:01:00", true });

            migrationBuilder.InsertData(
                table: "Sites",
                columns: new[] { "Id", "BaseUrl", "Name", "SettingsId" },
                values: new object[] { 1, "https://aliexpress.ru/", "AliExpress", 1 });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "IsDeleted", "Scheduler", "SiteId", "Url" },
                values: new object[] { 1, false, "[\"0 */30 * ? * *\"]", 1, "https://aliexpress.ru/item/4000561177801.html?spm=a2g0o.productlist.0.0.4c866c27X06Ss3&algo_pvid=e3004186-674b-47b9-9bfe-bde68786bf6c&algo_expid=e3004186-674b-47b9-9bfe-bde68786bf6c-1&btsid=0b8b15ea15903376264785085ed804&ws_ab_test=searchweb0_0,searchweb201602_,searchweb201603_" });

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_SiteSettings_SettingsId",
                table: "Sites",
                column: "SettingsId",
                principalTable: "SiteSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sites_SiteSettings_SettingsId",
                table: "Sites");

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Sites",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SiteSettings",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "SettingsId",
                table: "Sites",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Sites_SiteSettings_SettingsId",
                table: "Sites",
                column: "SettingsId",
                principalTable: "SiteSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
