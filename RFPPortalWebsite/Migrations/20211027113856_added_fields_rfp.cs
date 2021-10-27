using Microsoft.EntityFrameworkCore.Migrations;

namespace RFPPortalWebsite.Migrations
{
    public partial class added_fields_rfp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Amount",
                table: "Rfps",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Rfps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Rfps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Timeframe",
                table: "Rfps",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Rfps",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Rfps");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Rfps");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Rfps");

            migrationBuilder.DropColumn(
                name: "Timeframe",
                table: "Rfps");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Rfps");
        }
    }
}
