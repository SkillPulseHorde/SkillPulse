using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssessmentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnEvaluatorAssessmentInEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Evaluations_AssessmentId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_EvaluatorId",
                table: "Evaluations");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_AssessmentId_EvaluatorId",
                table: "Evaluations",
                columns: new[] { "AssessmentId", "EvaluatorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Evaluations_AssessmentId_EvaluatorId",
                table: "Evaluations");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_AssessmentId",
                table: "Evaluations",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_EvaluatorId",
                table: "Evaluations",
                column: "EvaluatorId");
        }
    }
}
