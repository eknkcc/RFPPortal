using Microsoft.EntityFrameworkCore.Migrations;

namespace RFPPortalWebsite.Migrations
{
    public partial class deleted_forminput : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FormInput",
                table: "Rfps");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FormInput",
                table: "Rfps",
                type: "text",
                nullable: true);
        }
    }
}
