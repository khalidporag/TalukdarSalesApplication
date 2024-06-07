using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalukdarSalesAPI.Migrations
{
    public partial class IsActive_property_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SalesRequisitionDetails");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SalesRequisitions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SalesRequisitions");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SalesRequisitionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
