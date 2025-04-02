using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Judicial_system.Migrations
{
    /// <inheritdoc />
    public partial class AddFileSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Tasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShareableLink",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ShareableLink",
                table: "Tasks");
        }
    }
}
