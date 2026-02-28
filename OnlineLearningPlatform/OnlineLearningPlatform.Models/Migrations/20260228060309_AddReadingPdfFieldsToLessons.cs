using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineLearningPlatform.Models.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingPdfFieldsToLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReadingPdfOriginalFileName",
                table: "Lessons",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReadingPdfStoragePath",
                table: "Lessons",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadingPdfOriginalFileName",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "ReadingPdfStoragePath",
                table: "Lessons");
        }
    }
}
