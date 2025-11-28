using AssessmentService.Domain.Entities;

namespace AssessmentService.Domain.Repos;

public interface IAssessmentRepository
{
    Task<Guid> CreateAsync(Assessment assessment, CancellationToken ct = default);

    Task<Assessment?> GetByIdReadonlyAsync(Guid assessmentId, CancellationToken ct = default);

    Task<Assessment?> GetByIdAsync(Guid assessmentId, CancellationToken ct = default);

    Task<Guid> UpdateAsync(Assessment assessment, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid assessmentId, CancellationToken ct = default);

    Task<List<Assessment>> GetAssessmentsReadonlyAsync(bool isActive, CancellationToken ct = default);

    Task<List<Assessment>> GetActiveAssessmentsByEvaluatorIdReadonlyAsync(Guid evaluatorId,
        CancellationToken ct = default);

    Task<List<Assessment>> GetCompletedAssessmentsByEvaluateeIdReadonlyAsync(Guid evaluateeId,
        CancellationToken ct = default);

    /// <summary>
    /// // Есть ли уже аттестация для данного пользователя за данный период
    /// </summary>
    Task<bool> HasOverlappingAssessmentAsync(Guid evaluateeId, DateTime startAt, DateTime endsAt, 
        CancellationToken ct = default);
}