using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class AddSkinInfoHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "SkinInfo",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SkinInfo_DeletePending",
                table: "SkinInfo",
                column: "DeletePending");

            migrationBuilder.CreateIndex(
                name: "IX_SkinInfo_Hash",
                table: "SkinInfo",
                column: "Hash",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SkinInfo_DeletePending",
                table: "SkinInfo");

            migrationBuilder.DropIndex(
                name: "IX_SkinInfo_Hash",
                table: "SkinInfo");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "SkinInfo");
        }
    }
}
