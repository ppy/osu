using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class KeyBindingActionReBind : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActionName",
                table: "KeyBinding",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KeyBinding_ActionName",
                table: "KeyBinding",
                column: "ActionName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_KeyBinding_ActionName",
                table: "KeyBinding");

            migrationBuilder.DropColumn(
                name: "ActionName",
                table: "KeyBinding");
        }
    }
}
