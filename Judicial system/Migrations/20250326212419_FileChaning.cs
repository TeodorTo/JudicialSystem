using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Judicial_system.Migrations
{
    /// <inheritdoc />
    public partial class FileChaning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "Tasks",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Tasks");
        }
    }
}
