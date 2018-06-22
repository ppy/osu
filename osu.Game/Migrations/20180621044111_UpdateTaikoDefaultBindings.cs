using Microsoft.EntityFrameworkCore.Migrations;
using osu.Framework.Logging;

namespace osu.Game.Migrations
{
    public partial class UpdateTaikoDefaultBindings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM KeyBinding WHERE RulesetID = 1");
            Logger.Log("osu!taiko bindings have been reset due to new defaults", LoggingTarget.Runtime, LogLevel.Important);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // we can't really tell if these should be restored or not, so let's just not do so.
        }
    }
}
