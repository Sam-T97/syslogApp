using Microsoft.EntityFrameworkCore.Migrations;

namespace syslogSite.Migrations
{
    public partial class FullAlertsRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alerts_Devices_DeviceID",
                table: "alerts");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceID",
                table: "alerts",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_alerts_Devices_DeviceID",
                table: "alerts",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alerts_Devices_DeviceID",
                table: "alerts");

            migrationBuilder.AlterColumn<int>(
                name: "DeviceID",
                table: "alerts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_alerts_Devices_DeviceID",
                table: "alerts",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
