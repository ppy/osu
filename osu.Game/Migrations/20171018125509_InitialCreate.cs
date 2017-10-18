using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace osu.Game.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeatmapSetInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeletePending = table.Column<bool>(type: "INTEGER", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", nullable: true),
                    OnlineBeatmapSetID = table.Column<int>(type: "INTEGER", nullable: true),
                    Protected = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapSetInfo", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FileInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hash = table.Column<string>(type: "TEXT", nullable: true),
                    ReferenceCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileInfo", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "KeyBinding",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    Keys = table.Column<string>(type: "TEXT", nullable: true),
                    RulesetID = table.Column<int>(type: "INTEGER", nullable: true),
                    Variant = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyBinding", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "RulesetInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Available = table.Column<bool>(type: "INTEGER", nullable: false),
                    InstantiationInfo = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RulesetInfo", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BeatmapSetFileInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BeatmapSetInfoID = table.Column<int>(type: "INTEGER", nullable: false),
                    FileInfoID = table.Column<int>(type: "INTEGER", nullable: false),
                    Filename = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapSetFileInfo", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BeatmapSetFileInfo_BeatmapSetInfo_BeatmapSetInfoID",
                        column: x => x.BeatmapSetInfoID,
                        principalTable: "BeatmapSetInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeatmapSetFileInfo_FileInfo_FileInfoID",
                        column: x => x.FileInfoID,
                        principalTable: "FileInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BeatmapInfo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AudioLeadIn = table.Column<int>(type: "INTEGER", nullable: false),
                    BaseDifficultyID = table.Column<int>(type: "INTEGER", nullable: false),
                    BeatDivisor = table.Column<int>(type: "INTEGER", nullable: false),
                    BeatmapSetInfoID = table.Column<int>(type: "INTEGER", nullable: false),
                    Countdown = table.Column<bool>(type: "INTEGER", nullable: false),
                    DistanceSpacing = table.Column<double>(type: "REAL", nullable: false),
                    GridSize = table.Column<int>(type: "INTEGER", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", nullable: true),
                    Hidden = table.Column<bool>(type: "INTEGER", nullable: false),
                    LetterboxInBreaks = table.Column<bool>(type: "INTEGER", nullable: false),
                    MD5Hash = table.Column<string>(type: "TEXT", nullable: true),
                    Path = table.Column<string>(type: "TEXT", nullable: true),
                    RulesetID = table.Column<int>(type: "INTEGER", nullable: false),
                    SpecialStyle = table.Column<bool>(type: "INTEGER", nullable: false),
                    StackLeniency = table.Column<float>(type: "REAL", nullable: false),
                    StarDifficulty = table.Column<double>(type: "REAL", nullable: false),
                    StoredBookmarks = table.Column<string>(type: "TEXT", nullable: true),
                    TimelineZoom = table.Column<double>(type: "REAL", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: true),
                    WidescreenStoryboard = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapInfo", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BeatmapInfo_BeatmapSetInfo_BeatmapSetInfoID",
                        column: x => x.BeatmapSetInfoID,
                        principalTable: "BeatmapSetInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeatmapInfo_RulesetInfo_RulesetID",
                        column: x => x.RulesetID,
                        principalTable: "RulesetInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BeatmapDifficulty",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApproachRate = table.Column<float>(type: "REAL", nullable: false),
                    BeatmapInfoID = table.Column<int>(type: "INTEGER", nullable: false),
                    CircleSize = table.Column<float>(type: "REAL", nullable: false),
                    DrainRate = table.Column<float>(type: "REAL", nullable: false),
                    OverallDifficulty = table.Column<float>(type: "REAL", nullable: false),
                    SliderMultiplier = table.Column<float>(type: "REAL", nullable: false),
                    SliderTickRate = table.Column<float>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapDifficulty", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BeatmapDifficulty_BeatmapInfo_BeatmapInfoID",
                        column: x => x.BeatmapInfoID,
                        principalTable: "BeatmapInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BeatmapMetadata",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Artist = table.Column<string>(type: "TEXT", nullable: true),
                    ArtistUnicode = table.Column<string>(type: "TEXT", nullable: true),
                    AudioFile = table.Column<string>(type: "TEXT", nullable: true),
                    Author = table.Column<string>(type: "TEXT", nullable: true),
                    BackgroundFile = table.Column<string>(type: "TEXT", nullable: true),
                    BeatmapInfoID = table.Column<int>(type: "INTEGER", nullable: true),
                    BeatmapSetInfoID = table.Column<int>(type: "INTEGER", nullable: true),
                    PreviewTime = table.Column<int>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    TitleUnicode = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapMetadata", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BeatmapMetadata_BeatmapInfo_BeatmapInfoID",
                        column: x => x.BeatmapInfoID,
                        principalTable: "BeatmapInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BeatmapMetadata_BeatmapSetInfo_BeatmapSetInfoID",
                        column: x => x.BeatmapSetInfoID,
                        principalTable: "BeatmapSetInfo",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapDifficulty_BeatmapInfoID",
                table: "BeatmapDifficulty",
                column: "BeatmapInfoID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_BeatmapSetInfoID",
                table: "BeatmapInfo",
                column: "BeatmapSetInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_Hash",
                table: "BeatmapInfo",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_MD5Hash",
                table: "BeatmapInfo",
                column: "MD5Hash");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_RulesetID",
                table: "BeatmapInfo",
                column: "RulesetID");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapMetadata_BeatmapInfoID",
                table: "BeatmapMetadata",
                column: "BeatmapInfoID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapMetadata_BeatmapSetInfoID",
                table: "BeatmapMetadata",
                column: "BeatmapSetInfoID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapSetFileInfo_BeatmapSetInfoID",
                table: "BeatmapSetFileInfo",
                column: "BeatmapSetInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapSetFileInfo_FileInfoID",
                table: "BeatmapSetFileInfo",
                column: "FileInfoID");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapSetInfo_DeletePending",
                table: "BeatmapSetInfo",
                column: "DeletePending");

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapSetInfo_Hash",
                table: "BeatmapSetInfo",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_FileInfo_Hash",
                table: "FileInfo",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileInfo_ReferenceCount",
                table: "FileInfo",
                column: "ReferenceCount");

            migrationBuilder.CreateIndex(
                name: "IX_KeyBinding_Action",
                table: "KeyBinding",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_KeyBinding_Variant",
                table: "KeyBinding",
                column: "Variant");

            migrationBuilder.CreateIndex(
                name: "IX_RulesetInfo_Available",
                table: "RulesetInfo",
                column: "Available");

            migrationBuilder.CreateIndex(
                name: "IX_RulesetInfo_InstantiationInfo",
                table: "RulesetInfo",
                column: "InstantiationInfo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RulesetInfo_Name",
                table: "RulesetInfo",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeatmapDifficulty");

            migrationBuilder.DropTable(
                name: "BeatmapMetadata");

            migrationBuilder.DropTable(
                name: "BeatmapSetFileInfo");

            migrationBuilder.DropTable(
                name: "KeyBinding");

            migrationBuilder.DropTable(
                name: "BeatmapInfo");

            migrationBuilder.DropTable(
                name: "FileInfo");

            migrationBuilder.DropTable(
                name: "BeatmapSetInfo");

            migrationBuilder.DropTable(
                name: "RulesetInfo");
        }
    }
}
