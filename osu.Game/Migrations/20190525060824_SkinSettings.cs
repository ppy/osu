using Microsoft.EntityFrameworkCore.Migrations;

namespace osu.Game.Migrations
{
    public partial class SkinSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"create table Settings_dg_tmp
            (
	            ID INTEGER not null
		            constraint PK_Settings
			            primary key autoincrement,
	            Key TEXT not null,
	            RulesetID INTEGER,
	            Value TEXT,
	            Variant INTEGER,
	            SkinInfoID int
		            constraint Settings_SkinInfo_ID_fk
			            references SkinInfo
				            on delete restrict
            );

            insert into Settings_dg_tmp(ID, Key, RulesetID, Value, Variant) select ID, Key, RulesetID, Value, Variant from Settings;

            drop table Settings;

            alter table Settings_dg_tmp rename to Settings;

            create index IX_Settings_RulesetID_Variant
	            on Settings (RulesetID, Variant);

            create index Settings_SkinInfoID_index
	            on Settings (SkinInfoID);

            ");
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
