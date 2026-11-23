using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BatchLab.Migrations
{
    /// <inheritdoc />
    public partial class AddProductStockPriceIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Products_StockQuantity_Price",
                table: "Products",
                columns: new[] { "StockQuantity", "Price" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_StockQuantity_Price",
                table: "Products");
        }
    }
}
