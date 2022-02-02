using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

namespace osu.Game.Migrations
{
    public partial class StandardizePaths : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string windowsStyle = @"\";
            string standardized = "/";

            // Escaping \ does not seem to be needed.
            migrationBuilder.Sql($"UPDATE `BeatmapInfo` SET `Path` = REPLACE(`Path`, '{windowsStyle}', '{standardized}')");
            migrationBuilder.Sql($"UPDATE `BeatmapMetadata` SET `AudioFile` = REPLACE(`AudioFile`, '{windowsStyle}', '{standardized}')");
            migrationBuilder.Sql($"UPDATE `BeatmapMetadata` SET `BackgroundFile` = REPLACE(`BackgroundFile`, '{windowsStyle}', '{standardized}')");
            migrationBuilder.Sql($"UPDATE `BeatmapSetFileInfo` SET `Filename` = REPLACE(`Filename`, '{windowsStyle}', '{standardized}')");
            migrationBuilder.Sql($"UPDATE `SkinFileInfo` SET `Filename` = REPLACE(`Filename`, '{windowsStyle}', '{standardized}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
