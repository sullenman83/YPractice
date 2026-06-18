using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_bookings_status",
                table: "bookings");

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    login = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    password = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.CheckConstraint("chk_users_loginLen", "LENGTH(login) >= 3");
                    table.CheckConstraint("chk_users_passwordLen", "LENGTH(password) > 0");
                    table.CheckConstraint("chk_users_role", "role IN('User', 'Admin')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_bookings_user_id",
                table: "bookings",
                column: "user_id");

            migrationBuilder.AddCheckConstraint(
                name: "chk_bookings_status",
                table: "bookings",
                sql: "status IN('Pending', 'Confirmed', 'Rejected', 'Cancelled')");

            migrationBuilder.CreateIndex(
                name: "ix_users_login",
                table: "users",
                column: "login",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_bookings_users_user_id",
                table: "bookings",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bookings_users_user_id",
                table: "bookings");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropIndex(
                name: "ix_bookings_user_id",
                table: "bookings");

            migrationBuilder.DropCheckConstraint(
                name: "chk_bookings_status",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "bookings");

            migrationBuilder.AddCheckConstraint(
                name: "chk_bookings_status",
                table: "bookings",
                sql: "status IN('Pending', 'Confirmed', 'Rejected', 'Processing')");
        }
    }
}
