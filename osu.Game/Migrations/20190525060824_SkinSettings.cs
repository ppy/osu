using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class SkinSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SkinInfoID",
                table: "Settings",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_SkinInfoID",
                table: "Settings",
                column: "SkinInfoID");

            // unsupported by sqlite

            // migrationBuilder.AddForeignKey(
            //     name: "FK_Settings_SkinInfo_SkinInfoID",
            //     table: "Settings",
            //     column: "SkinInfoID",
            //     principalTable: "SkinInfo",
            //     principalColumn: "ID",
            //     onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Settings_SkinInfo_SkinInfoID",
                table: "Settings");

            migrationBuilder.DropIndex(
                name: "IX_Settings_SkinInfoID",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "SkinInfoID",
                table: "Settings");
        }
    }
}
