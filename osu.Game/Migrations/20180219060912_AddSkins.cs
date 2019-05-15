using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class AddSkins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SkinInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Creator = table.Column<string>(type: "TEXT", nullable: true),
                    DeletePending = table.Column<bool>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinInfo", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SkinFileInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileInfoID = table.Column<int>(type: "INTEGER", nullable: false),
                    Filename = table.Column<string>(type: "TEXT", nullable: false),
                    SkinInfoID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkinFileInfo", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SkinFileInfo_FileInfo_FileInfoID",
                        column: x => x.FileInfoID,
                        principalTable: "FileInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SkinFileInfo_SkinInfo_SkinInfoID",
                        column: x => x.SkinInfoID,
                        principalTable: "SkinInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkinFileInfo_FileInfoID",
                table: "SkinFileInfo",
                column: "FileInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_SkinFileInfo_SkinInfoID",
                table: "SkinFileInfo",
                column: "SkinInfoID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkinFileInfo");

            migrationBuilder.DropTable(
                name: "SkinInfo");
        }
    }
}
