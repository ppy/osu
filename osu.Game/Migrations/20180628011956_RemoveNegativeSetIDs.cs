using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class RemoveNegativeSetIDs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // There was a change that beatmaps were being loaded with "-1" online IDs, which is completely incorrect.
            // This ensures there will not be unique key conflicts as a result of these incorrectly imported beatmaps.
            migrationBuilder.Sql("UPDATE BeatmapSetInfo SET OnlineBeatmapSetID = null WHERE OnlineBeatmapSetID <= 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
