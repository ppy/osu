using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class AddRulesetInfoShortName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "RulesetInfo",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RulesetInfo_ShortName",
                table: "RulesetInfo",
                column: "ShortName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RulesetInfo_ShortName",
                table: "RulesetInfo");

            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "RulesetInfo");
        }
    }
}
