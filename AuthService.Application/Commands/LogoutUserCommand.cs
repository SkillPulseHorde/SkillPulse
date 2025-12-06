using AuthService.Domain.Repos;
using Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Commands;

public sealed record LogoutUserCommand(Guid UserId) : IRequest<Result>;

public sealed class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Result>
{
    private readonly IAuthRepository _authRepository;

    public LogoutUserCommandHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _authRepository.GetUserByIdReadOnlyAsync(request.UserId, cancellationToken);
        if (user == null)
            return Result<string>.Failure(Error.NotFound("Пользователь не найден"));

        user.ClearRefreshToken();

        await _authRepository.UpdateRefreshTokenUserAsync(user, cancellationToken);

        return Result.Success();
    }
}

public class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().WithMessage("Не передан UserId")
            .NotEmpty().WithMessage("Передан пустой UserId");
    }
}