using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;

namespace AssessmentService.Infrastructure.Repos;

public class AssessmentRepository : IAssessmentRepository
{
    private readonly AssessmentDbContext _dbContext;

    public AssessmentRepository(AssessmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Guid> CreateAsync(Assessment assessment, CancellationToken ct = default)
    {
        assessment.CreatedAt = DateTime.UtcNow;
        await _dbContext.AddAsync(assessment, ct);
        await _dbContext.SaveChangesAsync(ct);
        return assessment.Id;
    }
}