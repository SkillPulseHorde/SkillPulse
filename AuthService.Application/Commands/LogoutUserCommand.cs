using AuthService.Domain.Repos;
using Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Commands;

public sealed record LogoutUserCommand(string UserId) : IRequest<Result<string>>;

public sealed class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Result<string>>
{
    private readonly IAuthRepository _authRepository;
    
    public LogoutUserCommandHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<Result<string>> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(request.UserId);
        
        var user = await _authRepository.GetUserByUserIdAsync(userId, cancellationToken);
        if (user  == null)
            return Result<string>.Failure(Error.NotFound("Общая ошибка"));
        
        user.ClearRefreshToken();
        
        await _authRepository.UpdateUserAsync(user, cancellationToken);
        
        return Result<string>.Success("");
    }
}

public class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("GUID необходим")
            .NotEmpty().WithMessage("GUID не должен быть пустым")
            .Must(IsValidGuid).WithMessage("Некорректный GUID");
    }

    private bool IsValidGuid(string userId)
    {
        return Guid.TryParse(userId, out _);
    }
}