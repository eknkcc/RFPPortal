using Microsoft.EntityFrameworkCore.Migrations;

namespace RFPPortalWebsite.Migrations
{
    public partial class AuthKeyDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthKey",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ThirdPartyKey",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ThirdPartyType",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WinnerRfpBidID",
                table: "Rfps");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthKey",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdPartyKey",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThirdPartyType",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinnerRfpBidID",
                table: "Rfps",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
