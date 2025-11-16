namespace AssessmentService.Domain.Repos;

public interface IUserEvaluatorRepository
{
    /// <summary>
    /// Получить список идентификаторов оценщиков для заданного пользователя (кто меня оценивает).
    /// </summary>
    Task<List<Guid>> GetEvaluatorIdsByUserIdAsync(Guid userId, CancellationToken ct = default);
    
    
    // /// <summary>
    // /// Получить список идентификаторов пользователей, для которых заданный пользователь является оценщиком (кого я могу оценить).
    // /// </summary>
    // Task<List<Guid>> GetUserIdsByEvaluatorIdAsync(Guid userId, CancellationToken ct = default);
    
    Task UpdateEvaluatorsForUserAsync(Guid userId, List<Guid> newEvaluatorIds, CancellationToken ct = default);
}