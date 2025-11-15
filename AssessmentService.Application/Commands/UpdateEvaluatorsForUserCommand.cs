using AssessmentService.Application.ServiceClientsAbstract;
using AssessmentService.Domain.Repos;
using Common;
using FluentValidation;
using MediatR;

namespace AssessmentService.Application.Commands;

public sealed record UpdateEvaluatorsForUserCommand : IRequest<Result>
{
    public required Guid UserId { get; init; }
    public required List<Guid> EvaluatorIds { get; init; }
}

public sealed class UpdateEvaluatorsForUserCommandHandler(
    IUserEvaluatorRepository userEvaluatorRepository,
    IUserServiceClient userServiceClient)
    : IRequestHandler<UpdateEvaluatorsForUserCommand, Result>
{

    public async Task<Result> Handle(UpdateEvaluatorsForUserCommand request, CancellationToken cancellationToken)
    {
        
        var allUserIdsToCheck = request.EvaluatorIds
            .Append(request.UserId)
            .ToList();
        
        // существуют ли рецензенты и текущий пользователь
        var areUsersExist = await userServiceClient.UsersExistAsync(allUserIdsToCheck, cancellationToken);
        if (!areUsersExist)
        {
            return Result.Failure(Error.NotFound("Заданные пользователи не существуют"));
        }

        await userEvaluatorRepository.UpdateEvaluatorsForUserAsync(request.UserId, request.EvaluatorIds, cancellationToken);

        return Result.Success();
    }
}

public sealed class UpdateEvaluatorsForUserCommandValidator : AbstractValidator<UpdateEvaluatorsForUserCommand>
{
    public UpdateEvaluatorsForUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.EvaluatorIds).NotNull();
        RuleForEach(x => x.EvaluatorIds)
            .NotEmpty().WithMessage("EvaluatorIds не должны содержать пустых GUID");
    }
}