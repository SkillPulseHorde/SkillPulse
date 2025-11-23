namespace AssessmentService.Domain.Repos;

public interface IUserEvaluatorRepository
{
    /// <summary>
    /// Получить список идентификаторов оценщиков для заданного пользователя (кто меня оценивает).
    /// </summary>
    Task<List<Guid>> GetEvaluatorIdsByUserIdAsync(Guid userId, CancellationToken ct = default);
    
    
    Task UpdateEvaluatorsForUserAsync(Guid userId, List<Guid> newEvaluatorIds, CancellationToken ct = default);
}