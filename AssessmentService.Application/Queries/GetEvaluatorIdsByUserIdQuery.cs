using Common;
using MediatR;
using AssessmentService.Domain.Repos;

namespace AssessmentService.Application.Queries;

public sealed record GetEvaluatorIdsByUserIdQuery(Guid UserId) : IRequest<Result<List<Guid>>>;

public sealed class GetEvaluatorIdsByUserIdQueryHandler : IRequestHandler<GetEvaluatorIdsByUserIdQuery, Result<List<Guid>>>
{
    private readonly IAssessmentRepository _repo;

    public GetEvaluatorIdsByUserIdQueryHandler(IAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<List<Guid>>> Handle(GetEvaluatorIdsByUserIdQuery request, CancellationToken ct)
    {
        var evaluatorIds = await _repo.GetEvaluatorIdsByUserIdAsync(request.UserId, ct);

        return Result<List<Guid>>.Success(evaluatorIds);
    }
}