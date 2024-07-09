using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalukdarSalesAPI.Migrations
{
    public partial class sequencial_userId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SequencialUserId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SequencialUserId",
                table: "Users");
        }
    }
}
