// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using osu.Game.Database;

namespace osu.Game.Migrations
{
    [DbContext(typeof(OsuDbContext))]
    [Migration("20211020081609_ResetSkinHashes")]
    public partial class ResetSkinHashes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"UPDATE SkinInfo SET Hash = null");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
