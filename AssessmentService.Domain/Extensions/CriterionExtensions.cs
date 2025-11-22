namespace AssessmentService.Domain.Extensions;

using Entities;

public static class CriterionExtensions
{
    private static readonly Guid LeadershipId = new("5f993bab-15ba-482f-b1aa-2ee2a7f087d5");
    private static readonly Guid MentoringId = new("16b575c8-952a-40b6-82ac-e8e0a332f353");
    private static readonly Guid PublicPresentationId = new("593356fa-5c26-46c5-89ba-1066a553ee13");
    
    // Id обязательного критерия -> минимальный уровень сотрудника, для которого этот критерий является обязательным
    private static readonly Dictionary<Guid, EmployeeGrade> MandatoryCriterionToMinimalGradeMap = new()
    {
        { LeadershipId, EmployeeGrade.M2 },
        { MentoringId, EmployeeGrade.M2 },
        { PublicPresentationId, EmployeeGrade.M3 }
    };
    
    public static bool IsMandatoryCriterion(this Criterion criterion, EmployeeGrade grade)
    {
        // Критерии уровня Core всегда обязательны
        if (criterion.Level == CriterionLevel.Core)
            return true;
        
        // Проверяем, является ли критерий обязательным для данного грейда
        if (MandatoryCriterionToMinimalGradeMap.TryGetValue(criterion.Id, out var minimalGrade))
            return grade >= minimalGrade;
        
        return false;
    }
}

