using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Judicial_system.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicAndRelationToTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the Topics table
            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                });

            // 2. Seed a default topic
            migrationBuilder.Sql("INSERT INTO Topics (Name, Description) VALUES ('General', 'Default topic for existing tasks')");

            // 3. Add TopicId column to Tasks table
            migrationBuilder.AddColumn<int>(
                name: "TopicId",
                table: "Tasks",
                type: "int",
                nullable: false,
                defaultValue: 1); // Set default value to 1, which will be the Id of the "General" topic

            // 4. Create index for TopicId
            migrationBuilder.CreateIndex(
                name: "IX_Tasks_TopicId",
                table: "Tasks",
                column: "TopicId");

            // 5. Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Topics_TopicId",
                table: "Tasks",
                column: "TopicId",
                principalTable: "Topics",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Topics_TopicId",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_TopicId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TopicId",
                table: "Tasks");
        }
    }
}