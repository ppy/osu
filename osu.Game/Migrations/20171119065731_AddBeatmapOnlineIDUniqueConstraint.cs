using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class AddBeatmapOnlineIDUniqueConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_OnlineBeatmapID",
                table: "BeatmapInfo",
                column: "OnlineBeatmapID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfo_OnlineBeatmapID",
                table: "BeatmapInfo");
        }
    }
}
