using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Modified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeScheduleHistories_EmployeeId",
                table: "EmployeeScheduleHistories");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeScheduleHistories_EmployeeId",
                table: "EmployeeScheduleHistories",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EmployeeScheduleHistories_EmployeeId",
                table: "EmployeeScheduleHistories");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeScheduleHistories_EmployeeId",
                table: "EmployeeScheduleHistories",
                column: "EmployeeId",
                unique: true);
        }
    }
}
