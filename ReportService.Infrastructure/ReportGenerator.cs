using ReportService.Application;
using ReportService.Application.Models;
using TemplateEngine.Docx;
using System.Text;

namespace ReportService.Infrastructure;

public class ReportGenerator : IReportGenerator
{
    private readonly string _templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "ReportTemplate.docx");

    public async Task<byte[]> GenerateReportAsync(AssessmentResultModel assessmentResult,
        CompetencesAndCriteriaNamesModel names,
        RecommendationsModel recommendations,
        byte[] chartImage,
        string employeeName,
        CancellationToken ct)
    {
        // CPU-bound операция генерации документа выполняется в отдельном потоке
        return await Task.Run(() => GenerateReport(assessmentResult, names, recommendations, chartImage, employeeName), ct);
    }

    private byte[] GenerateReport(
        AssessmentResultModel assessmentResult,
        CompetencesAndCriteriaNamesModel names,
        RecommendationsModel recommendations,
        byte[] chartImage,
        string employeeName)
    {
        try
        {
            if (!File.Exists(_templatePath))
                throw new FileNotFoundException($"Шаблон отчёта не найден: {_templatePath}");

            using var templateStream = File.OpenRead(_templatePath);
            using var generatedDoc = new MemoryStream();
            templateStream.CopyTo(generatedDoc);
            generatedDoc.Position = 0;

            using var processor = new TemplateProcessor(generatedDoc).SetRemoveContentControls(true);

            var recommendationText = ToDocxNewLines(BuildRecommendationsText(recommendations));

            var contentItems = new List<IContentItem>
            {
                new FieldContent("Recommendations", recommendationText),
                new FieldContent("EmployeeName", employeeName),
                new ImageContent("ChartImage", chartImage)
            };

            foreach (var (competenceId, competenceSummary) in assessmentResult.CompetenceSummaries)
            {
                if (competenceSummary is null
                    || !names.CompetenceNames.TryGetValue(competenceId, out var competenceName))
                {
                    continue;
                }

                var competenceCommentsText = string.Join(
                    "\r\n",
                    competenceSummary.Comments.Select(c => $"🗯️ {c}"));

                competenceCommentsText = ToDocxNewLines(competenceCommentsText);

                var competenceTag = $"{competenceName}_Comments";

                contentItems.Add(new FieldContent(competenceTag, competenceCommentsText));

                foreach (var (criterionId, criterionSummary) in competenceSummary.CriterionSummaries)
                {
                    if (!names.CriterionNames.TryGetValue(criterionId, out var criterionName))
                        continue;

                    var scoreTag = $"{criterionName}_Score";
                    contentItems.Add(new FieldContent(
                        scoreTag, criterionSummary.Score?.ToString("0.##") ?? "Ещё не оценивался"));

                    var commentsText = string.Join(
                        "\r\n",
                        criterionSummary.Comments.Select(c => $"🗣 {c}"));

                    commentsText = ToDocxNewLines(commentsText);

                    var commentsTag = $"{criterionName}_Comments";
                    contentItems.Add(new FieldContent(commentsTag, commentsText));
                }
            }

            var content = new Content(contentItems.ToArray());
            processor.FillContent(content);

            using var resultStream = new MemoryStream();
            processor.SaveChanges();

            generatedDoc.Position = 0;
            generatedDoc.CopyTo(resultStream);

            return resultStream.ToArray();
        }
        catch (FileNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка генерации отчёта.", ex);
        }
    }
    
    private string BuildRecommendationsText(RecommendationsModel recommendations)
    {
        if (recommendations.CompetenceRecommendations.Count == 0)
            return "Рекомендации отсутствуют.";

        var sb = new StringBuilder();

        foreach (var rec in recommendations.CompetenceRecommendations)
        {
            if (sb.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine("◆═════════════════════════════════════════════════════");
                sb.AppendLine();
            }

            sb.AppendLine($"Компетенция: {rec.CompetenceName}");

            if (!string.IsNullOrWhiteSpace(rec.CompetenceReason))
            {
                sb.AppendLine($"Важность: {rec.CompetenceReason}");
            }

            if (!rec.IsEvaluated)
            {
                sb.AppendLine("(Компетенция ещё не была оценена, рекомендаций пока нет).");
                continue;
            }

            
            // Способы развития компетенции
            if (!string.IsNullOrWhiteSpace(rec.WayToImproveCompetence))
            {
                var ways = rec.WayToImproveCompetence
                    .Split("§§", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (ways.Length > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("Способы развития компетенции:");

                    foreach (var way in ways)
                    {
                        sb.AppendLine($"• {way}");
                    }
                }
            }

            // Обучающие материалы
            if (rec.LearningMaterials is not { Count: > 0 })
                continue;
                
            sb.AppendLine();
            sb.AppendLine("Рекомендуемые материалы для изучения:");

            foreach (var material in rec.LearningMaterials)
            {
                var title = string.IsNullOrWhiteSpace(material.LearningMaterialName)
                    ? "Материал"
                    : material.LearningMaterialName;

                var typeRu = MapLearningMaterialTypeToRussian(material.LearningMaterialType);
                var type = string.IsNullOrWhiteSpace(typeRu)
                    ? string.Empty
                    : $" ({typeRu})";

                if (!string.IsNullOrWhiteSpace(material.LearningMaterialUrl))
                {
                    sb.AppendLine($"• {title}{type}: {material.LearningMaterialUrl}");
                }
                else
                {
                    sb.AppendLine($"• {title}{type}");
                }
            }
        }

        return sb.Length == 0 ? "Рекомендации отсутствуют." : sb.ToString().TrimEnd();
    }

    private static string MapLearningMaterialTypeToRussian(string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return string.Empty;

        return type.Trim() switch
        {
            "Video" => "Видео",
            "Book" => "Книга",
            "Course" => "Курс",
            _ => type
        };
    }
    
    // Нормализация переводов строк к формату CRLF, который ожидает Word/TemplateEngine.Docx
    private static string ToDocxNewLines(string text) =>
        string.IsNullOrEmpty(text)
            ? text
            : text.Replace("\r\n", "\n").Replace("\n", "\r\n");
}