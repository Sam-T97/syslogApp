using Microsoft.EntityFrameworkCore.Migrations;

namespace syslogSite.Migrations
{
    public partial class LinkDeviceToAlert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeviceID",
                table: "alerts",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_alerts_DeviceID",
                table: "alerts",
                column: "DeviceID");

            migrationBuilder.AddForeignKey(
                name: "FK_alerts_Devices_DeviceID",
                table: "alerts",
                column: "DeviceID",
                principalTable: "Devices",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_alerts_Devices_DeviceID",
                table: "alerts");

            migrationBuilder.DropIndex(
                name: "IX_alerts_DeviceID",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "DeviceID",
                table: "alerts");
        }
    }
}
