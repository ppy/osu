// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class KeyBindingDBUpgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn("KeyBinding", "Action", action =>
            {
                // don't think there is a way to do this without having access to a previous state of the keybinding enums or whatever they're using
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
