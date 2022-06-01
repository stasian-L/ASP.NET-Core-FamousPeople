using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamousPeople.Data.Migrations
{
    public partial class addinPerson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetailedBio",
                table: "People",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetailedBio",
                table: "People");
        }
    }
}
