using AssessmentService.Domain.Repos;
using Common;
using MediatR;
using FluentValidation;

namespace AssessmentService.Application.Commands;

public sealed record DeleteAssessmentCommand(Guid AssessmentId) : IRequest<Result<Guid>>;

public sealed class DeleteAssessmentCommandHandler(
    IAssessmentRepository assessmentRepository)
    : IRequestHandler<DeleteAssessmentCommand, Result<Guid>>
{
    private readonly DeleteAssessmentCommandValidator _validator = new();
    
    public async Task<Result<Guid>> Handle(DeleteAssessmentCommand request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken: ct);
        
        var deleted = await assessmentRepository.DeleteAsync(request.AssessmentId, ct);
        
        return !deleted 
            ? Result<Guid>.Failure(Error.NotFound("Аттестация не найдена")) 
            : Result<Guid>.Success(request.AssessmentId);
    }
}

public class DeleteAssessmentCommandValidator : AbstractValidator<DeleteAssessmentCommand>
{
    public DeleteAssessmentCommandValidator()
    {
        RuleFor(x => x.AssessmentId).NotEmpty();
    }
}