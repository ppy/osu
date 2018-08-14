using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class RemoveUniqueHashConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfo_Hash",
                table: "BeatmapInfo");

            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfo_MD5Hash",
                table: "BeatmapInfo");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_Hash",
                table: "BeatmapInfo",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_MD5Hash",
                table: "BeatmapInfo",
                column: "MD5Hash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfo_Hash",
                table: "BeatmapInfo");

            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfo_MD5Hash",
                table: "BeatmapInfo");

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
    }
}
