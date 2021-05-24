using Microsoft.EntityFrameworkCore.Migrations;

namespace PaqueteriasAYT.Migrations
{
    public partial class UpdateAppConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "AppConfiguration",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User",
                table: "AppConfiguration");
        }
    }
}
