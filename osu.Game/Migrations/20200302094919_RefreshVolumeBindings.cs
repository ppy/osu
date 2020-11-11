using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class RefreshVolumeBindings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM KeyBinding WHERE action in (6,7)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
