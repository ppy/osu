using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class Settings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KeyBinding_Variant",
                table: "KeyBinding");

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "TEXT", nullable: false),
                    RulesetID = table.Column<int>(type: "INTEGER", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    Variant = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KeyBinding_RulesetID_Variant",
                table: "KeyBinding",
                columns: new[] { "RulesetID", "Variant" });

            migrationBuilder.CreateIndex(
                name: "IX_Settings_RulesetID_Variant",
                table: "Settings",
                columns: new[] { "RulesetID", "Variant" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_KeyBinding_RulesetID_Variant",
                table: "KeyBinding");

            migrationBuilder.CreateIndex(
                name: "IX_KeyBinding_Variant",
                table: "KeyBinding",
                column: "Variant");
        }
    }
}
