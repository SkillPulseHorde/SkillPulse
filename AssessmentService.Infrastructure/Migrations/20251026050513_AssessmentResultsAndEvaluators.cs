using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssessmentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssessmentResultsAndEvaluators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentEvaluators",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentEvaluators", x => new { x.AssessmentId, x.EvaluatorId });
                    table.ForeignKey(
                        name: "FK_AssessmentEvaluators_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentResults",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentResults", x => x.AssessmentId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentEvaluators_EvaluatorId",
                table: "AssessmentEvaluators",
                column: "EvaluatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentEvaluators");

            migrationBuilder.DropTable(
                name: "AssessmentResults");
        }
    }
}
