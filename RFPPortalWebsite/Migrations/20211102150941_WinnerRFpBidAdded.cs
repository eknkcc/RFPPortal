using Microsoft.EntityFrameworkCore.Migrations;

namespace RFPPortalWebsite.Migrations
{
    public partial class WinnerRFpBidAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WinnerRfpBidID",
                table: "Rfps",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WinnerRfpBidID",
                table: "Rfps");
        }
    }
}
