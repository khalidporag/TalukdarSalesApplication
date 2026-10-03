using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalukdarSales.Web.Migrations
{
    public partial class propertyTypeChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "CollectionAmount",
                table: "SalesInvoices",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CollectionAmount",
                table: "SalesInvoices",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }
    }
}
