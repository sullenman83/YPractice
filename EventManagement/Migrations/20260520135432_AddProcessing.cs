using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_bookings_status",
                table: "bookings");

            migrationBuilder.RenameColumn(
                name: "processed_at",
                table: "bookings",
                newName: "processing_at");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessingAt",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "chk_bookings_status",
                table: "bookings",
                sql: "status IN('Pending', 'Confirmed', 'Rejected', 'Processing')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_bookings_status",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "ProcessingAt",
                table: "bookings");

            migrationBuilder.RenameColumn(
                name: "processing_at",
                table: "bookings",
                newName: "processed_at");

            migrationBuilder.AddCheckConstraint(
                name: "chk_bookings_status",
                table: "bookings",
                sql: "status IN('Pending', 'Confirmed', 'Rejected')");
        }
    }
}
