// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class AddAuthorIdToBeatmapMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AuthorID",
                table: "BeatmapMetadata",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorID",
                table: "BeatmapMetadata");
        }
    }
}
