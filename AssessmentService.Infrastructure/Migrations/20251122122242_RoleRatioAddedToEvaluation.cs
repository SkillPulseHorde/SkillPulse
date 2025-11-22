using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssessmentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RoleRatioAddedToEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleRatio",
                table: "Evaluations",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoleRatio",
                table: "Evaluations");
        }
    }
}
