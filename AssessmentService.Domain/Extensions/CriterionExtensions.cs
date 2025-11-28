namespace AssessmentService.Domain.Extensions;

using Entities;

public static class CriterionExtensions
{
    // Название обязательного критерия -> минимальный уровень сотрудника, для которого этот критерий является обязательным
    private static readonly Dictionary<string, EmployeeGrade> MandatoryCriterionToMinimalGradeMap = new()
    {
        { "Лидерские качества/авторитет", EmployeeGrade.M2 },
        { "Проактивно менторит", EmployeeGrade.M2 },
        { "Публично выступает", EmployeeGrade.M3 }
    };
    
    public static bool IsMandatoryCriterion(this Criterion criterion, EmployeeGrade grade)
    {
        // Критерии уровня Core всегда обязательны
        if (criterion.Level == CriterionLevel.Core)
            return true;
        
        // Проверяем, является ли критерий обязательным для данного грейда
        if (MandatoryCriterionToMinimalGradeMap.TryGetValue(criterion.Name, out var minimalGrade))
            return grade >= minimalGrade;
        
        return false;
    }
}

