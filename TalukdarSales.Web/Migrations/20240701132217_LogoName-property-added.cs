using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalukdarSales.Web.Migrations
{
    public partial class LogoNamepropertyadded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoName",
                table: "FinishedGoods",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LogoName",
                table: "FinishedGoods");
        }
    }
}
