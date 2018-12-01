using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class AddScoreInfoTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoreInfo",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Rank = table.Column<int>(nullable: false),
                    TotalScore = table.Column<int>(nullable: false),
                    Accuracy = table.Column<double>(type: "DECIMAL(1,4)", nullable: false),
                    PP = table.Column<double>(nullable: true),
                    MaxCombo = table.Column<int>(nullable: false),
                    Combo = table.Column<int>(nullable: false),
                    RulesetID = table.Column<int>(nullable: false),
                    Mods = table.Column<string>(nullable: true),
                    User = table.Column<string>(nullable: true),
                    BeatmapInfoID = table.Column<int>(nullable: false),
                    OnlineScoreID = table.Column<long>(nullable: true),
                    Date = table.Column<DateTimeOffset>(nullable: false),
                    Statistics = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true),
                    DeletePending = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreInfo", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ScoreInfo_BeatmapInfo_BeatmapInfoID",
                        column: x => x.BeatmapInfoID,
                        principalTable: "BeatmapInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScoreInfo_RulesetInfo_RulesetID",
                        column: x => x.RulesetID,
                        principalTable: "RulesetInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScoreFileInfo",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileInfoID = table.Column<int>(nullable: false),
                    Filename = table.Column<string>(nullable: false),
                    ScoreInfoID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreFileInfo", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ScoreFileInfo_FileInfo_FileInfoID",
                        column: x => x.FileInfoID,
                        principalTable: "FileInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScoreFileInfo_ScoreInfo_ScoreInfoID",
                        column: x => x.ScoreInfoID,
                        principalTable: "ScoreInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScoreFileInfo_FileInfoID",
                table: "ScoreFileInfo",
                column: "FileInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreFileInfo_ScoreInfoID",
                table: "ScoreFileInfo",
                column: "ScoreInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreInfo_BeatmapInfoID",
                table: "ScoreInfo",
                column: "BeatmapInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreInfo_OnlineScoreID",
                table: "ScoreInfo",
                column: "OnlineScoreID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScoreInfo_RulesetID",
                table: "ScoreInfo",
                column: "RulesetID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScoreFileInfo");

            migrationBuilder.DropTable(
                name: "ScoreInfo");
        }
    }
}
