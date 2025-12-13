using AssessmentService.Domain.Entities;
using AssessmentService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssessmentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompetenceSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var competence in CompetenceSeed.Data)
            {
                var compId = Guid.NewGuid();

                // Вставляем компетенцию с описанием
                migrationBuilder.Sql($@"
                    INSERT INTO ""Competences"" (""Id"", ""Name"", ""Description"")
                    VALUES ('{compId}', '{competence.Name.Replace("'", "''")}', '{competence.Description.Replace("'", "''")}');");

                // Вставляем критерии уровня Core
                foreach (var name in competence.Core)
                {
                    migrationBuilder.Sql($@"
                        INSERT INTO ""Criteria"" (""Id"", ""CompetenceId"", ""Name"", ""Level"")
                        VALUES ('{Guid.NewGuid()}', '{compId}', '{name.Replace("'", "''")}', 'Core');");
                }

                // Вставляем критерии уровня Advanced
                foreach (var name in competence.Advanced)
                {
                    migrationBuilder.Sql($@"
                        INSERT INTO ""Criteria"" (""Id"", ""CompetenceId"", ""Name"", ""Level"")
                        VALUES ('{Guid.NewGuid()}', '{compId}', '{name.Replace("'", "''")}', 'Advanced');");
                }
            }
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Criteria;");
            migrationBuilder.Sql("DELETE FROM Competences;");
        }
    }
}