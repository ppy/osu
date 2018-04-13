using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class AddMissingIndexRules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BeatmapSetInfo_Hash",
                table: "BeatmapSetInfo");

            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfo_Hash",
                table: "BeatmapInfo");

            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfo_MD5Hash",
                table: "BeatmapInfo");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapSetInfo_Hash",
                table: "BeatmapSetInfo",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapSetInfo_OnlineBeatmapSetID",
                table: "BeatmapSetInfo",
                column: "OnlineBeatmapSetID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_Hash",
                table: "BeatmapInfo",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_MD5Hash",
                table: "BeatmapInfo",
                column: "MD5Hash",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BeatmapSetInfo_Hash",
                table: "BeatmapSetInfo");

            migrationBuilder.DropIndex(
                name: "IX_BeatmapSetInfo_OnlineBeatmapSetID",
                table: "BeatmapSetInfo");

            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfo_Hash",
                table: "BeatmapInfo");

            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfo_MD5Hash",
                table: "BeatmapInfo");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapSetInfo_Hash",
                table: "BeatmapSetInfo",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_Hash",
                table: "BeatmapInfo",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_MD5Hash",
                table: "BeatmapInfo",
                column: "MD5Hash");
        }
    }
}
