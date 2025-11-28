using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

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
                    Competence = table.Column<string>(type: "text", nullable: false),
                    Tag = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningMaterials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningMaterials_Competence",
                table: "LearningMaterials",
                column: "Competence");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndividualDevelopmentPlans");

            migrationBuilder.DropTable(
                name: "LearningMaterials");
        }
    }
}
