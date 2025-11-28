using MediatR;
using Common;
using RecommendationService.Application.AI;
using RecommendationService.Domain.AI;
using RecommendationService.Domain.Repos;

namespace RecommendationService.Application.Commands;

public sealed record GetRecommendationsByUserIdCommand(Guid UserId) : IRequest<Result<string>>;

public sealed class GetRecommendationsByUserIdCommandHandler : IRequestHandler<GetRecommendationsByUserIdCommand, Result<string>>
{
    //private readonly IIndividualDevelopmentPlanRepository _planRepository;
    private readonly IAiPlanGeneratorService _aiPlanGeneratorService;

    public GetRecommendationsByUserIdCommandHandler(
        IIndividualDevelopmentPlanRepository planRepository,
        IAiPlanGeneratorService aiPlanGeneratorService)
    {
//        _planRepository = planRepository;
        _aiPlanGeneratorService = aiPlanGeneratorService;
    }
    
    // public async Task<Result<RecommendationModel>> Handle(GetRecommendationsByUserIdCommand request,
    //     CancellationToken cancellationToken)
    // {
    //
    //     return Result<RecommendationModel>.Success(null);//
    // }
    
    public async Task<Result<string>> Handle(GetRecommendationsByUserIdCommand request,
        CancellationToken ct)
    {
        var answer = await _aiPlanGeneratorService.GeneratePlanAsync(
            "Если я на марафоне обгоню бегуна, который бежит вторым, какое я буду занимать место?", 
            ct);
        
        return Result<string>.Success(answer.Value);
    }
}