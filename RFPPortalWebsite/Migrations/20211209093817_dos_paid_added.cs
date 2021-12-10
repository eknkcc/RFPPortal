using Microsoft.EntityFrameworkCore.Migrations;

namespace RFPPortalWebsite.Migrations
{
    public partial class dos_paid_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DosPaid",
                table: "RfpBids",
                type: "tinyint(1)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DosPaid",
                table: "RfpBids");
        }
    }
}
