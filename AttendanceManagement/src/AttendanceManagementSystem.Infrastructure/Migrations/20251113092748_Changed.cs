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
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeSummaries_Employees_EmployeeId1",
                table: "EmployeeSummaries");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeSummaries_EmployeeId1",
                table: "EmployeeSummaries");

            migrationBuilder.DropColumn(
                name: "EmployeeId1",
                table: "EmployeeSummaries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "EmployeeId1",
                table: "EmployeeSummaries",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSummaries_EmployeeId1",
                table: "EmployeeSummaries",
                column: "EmployeeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeSummaries_Employees_EmployeeId1",
                table: "EmployeeSummaries",
                column: "EmployeeId1",
                principalTable: "Employees",
                principalColumn: "EmployeeId");
        }
    }
}
