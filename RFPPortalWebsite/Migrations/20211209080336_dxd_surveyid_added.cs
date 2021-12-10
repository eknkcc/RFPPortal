using Microsoft.EntityFrameworkCore.Migrations;

namespace RFPPortalWebsite.Migrations
{
    public partial class dxd_surveyid_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InternalSurveyId",
                table: "Rfps",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PublicSurveyId",
                table: "Rfps",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InternalSurveyId",
                table: "Rfps");

            migrationBuilder.DropColumn(
                name: "PublicSurveyId",
                table: "Rfps");
        }
    }
}
