using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace syslogSite.Migrations
{
    public partial class variablestableadded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Alerts",
                table: "Alerts");

            migrationBuilder.RenameTable(
                name: "Alerts",
                newName: "alerts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_alerts",
                table: "alerts",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "appvars",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VariableName = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appvars", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "appvars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_alerts",
                table: "alerts");

            migrationBuilder.RenameTable(
                name: "alerts",
                newName: "Alerts");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Alerts",
                table: "Alerts",
                column: "ID");
        }
    }
}
