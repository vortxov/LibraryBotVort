using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryBot.Migrations
{
    public partial class sizeFilev4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Length",
                table: "PathBooks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Length",
                table: "PathBooks");
        }
    }
}
