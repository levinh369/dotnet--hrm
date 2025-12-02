using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class updateDb1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceModels_Employees_EmployeeId",
                table: "AttendanceModels");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AttendanceModels",
                table: "AttendanceModels");

            migrationBuilder.RenameTable(
                name: "AttendanceModels",
                newName: "Attendances");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceModels_EmployeeId",
                table: "Attendances",
                newName: "IX_Attendances_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attendances_Employees_EmployeeId",
                table: "Attendances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Attendances",
                table: "Attendances");

            migrationBuilder.RenameTable(
                name: "Attendances",
                newName: "AttendanceModels");

            migrationBuilder.RenameIndex(
                name: "IX_Attendances_EmployeeId",
                table: "AttendanceModels",
                newName: "IX_AttendanceModels_EmployeeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AttendanceModels",
                table: "AttendanceModels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceModels_Employees_EmployeeId",
                table: "AttendanceModels",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
