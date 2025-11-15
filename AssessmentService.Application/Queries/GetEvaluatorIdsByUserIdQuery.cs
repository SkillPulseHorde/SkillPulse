using Common;
using MediatR;
using AssessmentService.Domain.Repos;

namespace AssessmentService.Application.Queries;

public sealed record GetEvaluatorIdsByUserIdQuery(Guid UserId) : IRequest<Result<List<Guid>>>;

public sealed class GetEvaluatorIdsByUserIdQueryHandler(
    IUserEvaluatorRepository userEvaluatorRepository) 
    : IRequestHandler<GetEvaluatorIdsByUserIdQuery, Result<List<Guid>>>
{
    public async Task<Result<List<Guid>>> Handle(GetEvaluatorIdsByUserIdQuery request, CancellationToken ct)
    {
        var evaluatorIds = await userEvaluatorRepository.GetEvaluatorIdsByUserIdAsync(request.UserId, ct);

        return Result<List<Guid>>.Success(evaluatorIds);
    }
}