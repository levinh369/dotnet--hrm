using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class updateDB2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Salaries_Employees_UserId",
                table: "Salaries");

            migrationBuilder.DropIndex(
                name: "IX_Salaries_UserId",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Salaries");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Salaries",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Bonus",
                table: "Salaries",
                newName: "NetSalary");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "Salaries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ActualWorkDays",
                table: "Salaries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Salaries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "Salaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "StandardWorkDays",
                table: "Salaries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "Allowance",
                table: "Contracts",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Salaries_EmployeeId",
                table: "Salaries",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Salaries_Employees_EmployeeId",
                table: "Salaries",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Salaries_Employees_EmployeeId",
                table: "Salaries");

            migrationBuilder.DropIndex(
                name: "IX_Salaries_EmployeeId",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "ActualWorkDays",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "StandardWorkDays",
                table: "Salaries");

            migrationBuilder.DropColumn(
                name: "Allowance",
                table: "Contracts");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Salaries",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "NetSalary",
                table: "Salaries",
                newName: "Bonus");

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "Salaries",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Salaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Salaries_UserId",
                table: "Salaries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Salaries_Employees_UserId",
                table: "Salaries",
                column: "UserId",
                principalTable: "Employees",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
