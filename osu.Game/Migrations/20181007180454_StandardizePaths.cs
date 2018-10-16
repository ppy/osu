using System;
using Microsoft.EntityFrameworkCore.Migrations;
using System.IO;

namespace osu.Game.Migrations
{
    public partial class StandardizePaths : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sanitized = Path.DirectorySeparatorChar.ToString();
            string standardized = "/";

            // Escaping \ does not seem to be needed.
            migrationBuilder.Sql($"UPDATE `BeatmapInfo` SET `Path` = REPLACE(`Path`, '{sanitized}', '{standardized}')");
            migrationBuilder.Sql($"UPDATE `BeatmapMetadata` SET `AudioFile` = REPLACE(`AudioFile`, '{sanitized}', '{standardized}')");
            migrationBuilder.Sql($"UPDATE `BeatmapMetadata` SET `BackgroundFile` = REPLACE(`BackgroundFile`, '{sanitized}', '{standardized}')");
            migrationBuilder.Sql($"UPDATE `BeatmapSetFileInfo` SET `Filename` = REPLACE(`Filename`, '{sanitized}', '{standardized}')");
            migrationBuilder.Sql($"UPDATE `SkinFileInfo` SET `Filename` = REPLACE(`Filename`, '{sanitized}', '{standardized}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
