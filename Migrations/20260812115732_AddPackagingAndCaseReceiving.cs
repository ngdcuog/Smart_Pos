using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPOS.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagingAndCaseReceiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImportUnitNameSnapshot",
                table: "StockTransactions",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImportUnitQuantity",
                table: "StockTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LooseUnitQuantity",
                table: "StockTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitCostSnapshot",
                table: "StockTransactions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitsPerImportUnitSnapshot",
                table: "StockTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaseBarcode",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportUnitName",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Thùng");

            migrationBuilder.AddColumn<string>(
                name: "RetailUnitName",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Cái");

            migrationBuilder.AddColumn<int>(
                name: "UnitsPerImportUnit",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { "18934588012228", "Thùng", "Chai", 24 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { "18934588012229", "Thùng", "Lon", 24 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { "18934588012230", "Thùng", "Chai", 24 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { "18934588012231", "Thùng", "Gói", 30 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 5,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { null, "Thùng", "Cái", 1 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 6,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { null, "Thùng", "Cái", 1 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 7,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { null, "Thùng", "Cái", 1 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 8,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { null, "Thùng", "Cái", 1 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 9,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { null, "Thùng", "Cái", 1 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 10,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { null, "Thùng", "Cái", 1 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 11,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { null, "Thùng", "Cái", 1 });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 12,
                columns: new[] { "CaseBarcode", "ImportUnitName", "RetailUnitName", "UnitsPerImportUnit" },
                values: new object[] { null, "Thùng", "Cái", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CaseBarcode",
                table: "Products",
                column: "CaseBarcode",
                unique: true,
                filter: "[CaseBarcode] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_UnitsPerImportUnit",
                table: "Products",
                sql: "[UnitsPerImportUnit] >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CaseBarcode",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_UnitsPerImportUnit",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImportUnitNameSnapshot",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "ImportUnitQuantity",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "LooseUnitQuantity",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "UnitCostSnapshot",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "UnitsPerImportUnitSnapshot",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "CaseBarcode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImportUnitName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RetailUnitName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitsPerImportUnit",
                table: "Products");
        }
    }
}
