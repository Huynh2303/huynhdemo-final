using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Demo_web_MVC.Migrations
{
    /// <inheritdoc />
    public partial class FraudAnalysisUpdateNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fraudanalysis_order",
                table: "FraudAnalysis");

            migrationBuilder.DropIndex(
                name: "idx_fraudanalysis_orderid",
                table: "FraudAnalysis");

            migrationBuilder.AlterColumn<decimal>(
                name: "RiskScore",
                table: "FraudAnalysis",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(4,3)");

            migrationBuilder.AddColumn<string>(
                name: "InputSnapshot",
                table: "FraudAnalysis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "FraudAnalysis",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RiskReasons",
                table: "FraudAnalysis",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_FraudAnalysis_OrderId",
                table: "FraudAnalysis",
                column: "OrderId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_fraudanalysis_order",
                table: "FraudAnalysis",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_fraudanalysis_order",
                table: "FraudAnalysis");

            migrationBuilder.DropIndex(
                name: "IX_FraudAnalysis_OrderId",
                table: "FraudAnalysis");

            migrationBuilder.DropColumn(
                name: "InputSnapshot",
                table: "FraudAnalysis");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "FraudAnalysis");

            migrationBuilder.DropColumn(
                name: "RiskReasons",
                table: "FraudAnalysis");

            migrationBuilder.AlterColumn<decimal>(
                name: "RiskScore",
                table: "FraudAnalysis",
                type: "decimal(4,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.CreateIndex(
                name: "idx_fraudanalysis_orderid",
                table: "FraudAnalysis",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "fk_fraudanalysis_order",
                table: "FraudAnalysis",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
