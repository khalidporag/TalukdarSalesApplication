using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalukdarSales.Web.Migrations
{
    public partial class invoiceSerialNo_property_added : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InvoiceSerialNo",
                table: "SalesInvoices",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoiceSerialNo",
                table: "SalesInvoices");
        }
    }
}
