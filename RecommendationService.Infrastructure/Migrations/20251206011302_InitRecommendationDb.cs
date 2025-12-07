using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RecommendationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitRecommendationDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IndividualDevelopmentPlans",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    SummaryJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualDevelopmentPlans", x => x.AssessmentId);
                });

            migrationBuilder.CreateTable(
                name: "LearningMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    CompetenceName = table.Column<string>(type: "text", nullable: false),
                    Tag = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningMaterials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThresholdValues",
                columns: table => new
                {
                    Grade = table.Column<string>(type: "text", nullable: false),
                    MinAvgCompetence = table.Column<double>(type: "double precision", nullable: false),
                    MinAvgCriterion = table.Column<double>(type: "double precision", nullable: false),
                    MinCoreThreshold = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThresholdValues", x => x.Grade);
                });

            migrationBuilder.InsertData(
                table: "ThresholdValues",
                columns: new[] { "Grade", "MinAvgCompetence", "MinAvgCriterion", "MinCoreThreshold" },
                values: new object[,]
                {
                    { "J1", 5.0, 3.0, 5.0 },
                    { "J2", 5.5, 4.0, 6.0 },
                    { "J3", 6.0, 5.0, 6.0 },
                    { "M1", 7.0, 6.0, 7.0 },
                    { "M2", 7.5, 7.0, 7.0 },
                    { "M3", 8.0, 7.0, 8.0 },
                    { "S", 8.5, 8.0, 8.0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningMaterials_CompetenceName",
                table: "LearningMaterials",
                column: "CompetenceName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndividualDevelopmentPlans");

            migrationBuilder.DropTable(
                name: "LearningMaterials");

            migrationBuilder.DropTable(
                name: "ThresholdValues");
        }
    }
}
