using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using osu.Game.Database;

namespace osu.Game.Migrations
{
    [DbContext(typeof(OsuDbContext))]
    [Migration("20211227135909_AddReplayGainData")]
    public partial class AddReplayGainData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*migrationBuilder.AddColumn<int>(
                name: "ReplayGainInfoID",
                table: "BeatmapInfo",
                nullable: false,
                defaultValue: 0
                );*/

           

            migrationBuilder.CreateTable(
                name: "ReplayGainInfo",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackGain = table.Column<float>(nullable: false),
                    PeakAmplitude = table.Column<float>(nullable: false),
                    Version = table.Column<float>(nullable: false),
                    DeletePending = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayGainInfo", x => x.ID);
                });

            /*migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfo_ReplayGainInfoID",
                table: "BeatmapInfo",
                column: "ReplayGainInfoID"
                );*/
            migrationBuilder.Sql(@"ALTER TABLE BeatmapInfo
	            ADD ReplayGainInfoID INTEGER NOT NULL DEFAULT 0 CONSTRAINT
                FK_BeatmapInfo_ReplayGainInfo_ReplayGainInfoID REFERENCES ReplayGainInfo(ID)
                ON DELETE CASCADE");
        }
        protected override void Down(MigrationBuilder migrationBuilder)
        { }
    }
}
