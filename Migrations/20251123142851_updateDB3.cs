using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DACN.Migrations
{
    /// <inheritdoc />
    public partial class updateDB3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EditReason",
                table: "Attendances",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EditTime",
                table: "Attendances",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EditedBy",
                table: "Attendances",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManuallyEdited",
                table: "Attendances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "RawCheckInTime",
                table: "Attendances",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "RawCheckOutTime",
                table: "Attendances",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditReason",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "EditTime",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "EditedBy",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "IsManuallyEdited",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "RawCheckInTime",
                table: "Attendances");

            migrationBuilder.DropColumn(
                name: "RawCheckOutTime",
                table: "Attendances");
        }
    }
}
