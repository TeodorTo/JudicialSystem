using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Judicial_system.Migrations
{
    /// <inheritdoc />
    public partial class added_File_Path_property : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Tasks");
        }
    }
}
