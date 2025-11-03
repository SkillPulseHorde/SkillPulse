using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssessmentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssessmentDatesIndexAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Assessments_StartAt_EndsAt",
                table: "Assessments",
                columns: new[] { "StartAt", "EndsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Assessments_StartAt_EndsAt",
                table: "Assessments");
        }
    }
}