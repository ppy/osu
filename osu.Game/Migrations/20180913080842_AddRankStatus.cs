// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class AddRankStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BeatmapSetInfo",
                nullable: false,
                defaultValue: -3); // NONE

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "BeatmapInfo",
                nullable: false,
                defaultValue: -3); // NONE
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BeatmapSetInfo");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "BeatmapInfo");
        }
    }
}
