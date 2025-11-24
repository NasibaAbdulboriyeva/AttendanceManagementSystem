using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Changed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_CardNumber",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "CardNumber",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CardId",
                table: "Employees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "FingerprintId",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FingerprintNumber",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CardNumber",
                table: "Employees",
                column: "CardNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_FingerprintNumber",
                table: "Employees",
                column: "FingerprintNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_CardNumber",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_FingerprintNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "FingerprintId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "FingerprintNumber",
                table: "Employees");

            migrationBuilder.AlterColumn<int>(
                name: "CardNumber",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CardId",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CardNumber",
                table: "Employees",
                column: "CardNumber",
                unique: true);
        }
    }
}
