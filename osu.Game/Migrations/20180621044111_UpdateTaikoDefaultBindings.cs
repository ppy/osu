// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class UpdateTaikoDefaultBindings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM KeyBinding WHERE RulesetID = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // we can't really tell if these should be restored or not, so let's just not do so.
        }
    }
}
