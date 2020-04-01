using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace syslogSite.Migrations
{
    public partial class AddedMailingListTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MailingLists",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ListName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailingLists", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MailingListMembers",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(nullable: false),
                    MailingListID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailingListMembers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MailingListMembers_MailingLists_MailingListID",
                        column: x => x.MailingListID,
                        principalTable: "MailingLists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MailingListMembers_MailingListID",
                table: "MailingListMembers",
                column: "MailingListID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MailingListMembers");

            migrationBuilder.DropTable(
                name: "MailingLists");
        }
    }
}
