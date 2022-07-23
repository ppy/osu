// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using osu.Game.Database;
using osu.Game.Input.Bindings;

namespace osu.Game.Migrations
{
    [DbContext(typeof(OsuDbContext))]
    [Migration("20180131154205_AddMuteBinding")]
    public partial class AddMuteBinding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"UPDATE KeyBinding SET Action = Action + 1 WHERE RulesetID IS NULL AND Variant IS NULL AND Action >= {(int)GlobalAction.ToggleMute}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DELETE FROM KeyBinding WHERE RulesetID IS NULL AND Variant IS NULL AND Action = {(int)GlobalAction.ToggleMute}");
            migrationBuilder.Sql($"UPDATE KeyBinding SET Action = Action - 1 WHERE RulesetID IS NULL AND Variant IS NULL AND Action > {(int)GlobalAction.ToggleMute}");
        }
    }
}
