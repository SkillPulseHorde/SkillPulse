using AuthService.Application.interfaces;
using AuthService.Application.Models;
using AuthService.Domain.Entities;
using AuthService.Domain.Repos;
using MediatR;
using Common;
using FluentValidation;

namespace AuthService.Application.Commands;

public sealed record AuthenticateUserCommand(string Email, string Password) : IRequest<Result<TokenResponse>>;

public sealed class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, Result<TokenResponse>>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthRepository _authRepository;

    public AuthenticateUserCommandHandler(IJwtProvider jwtProvider, IPasswordHasher passwordHasher,
        IAuthRepository authRepository)
    {
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
        _authRepository = authRepository;
    }

    public async Task<Result<TokenResponse>> Handle(AuthenticateUserCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return Result<TokenResponse>.Failure(Error.Unauthorized("Не удалось войти в систему"));

        var result = _passwordHasher.IsPasswordValid(request.Password, user.PasswordHash);
        if (!result)
            return Result<TokenResponse>.Failure(Error.Unauthorized("Не удалось войти в систему"));

        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        user.SetRefreshToken(
            refreshToken,
            DateTimeOffset.UtcNow.AddHours(_jwtProvider.GetRefreshExpiresHours())
        );

        await _authRepository.UpdateUserAsync(user, cancellationToken);

        var tokenResponse = new TokenResponse()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };

        return Result<TokenResponse>.Success(tokenResponse);
    }
}

public class AuthenticateUserCommandValidator : AbstractValidator<AuthenticateUserCommand>
{
    public AuthenticateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotNull().WithMessage("Не передан Email")
            .NotEmpty().WithMessage("Передан пустой Email")
            .EmailAddress().WithMessage("Некорректный формат Email.")
            .MaximumLength(255).WithMessage("Email должен быть не более 255 символов.");
        RuleFor(x => x.Password)
            .NotNull().WithMessage("Не передан Password")
            .NotEmpty().WithMessage("Передан пустой Password")
            .MinimumLength(4).WithMessage("Password должен быть не менее 4 символов.")
            .MaximumLength(512).WithMessage("Password должен быть не более 512 символов.")
            .Matches(@"^(?!.*\s)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).+$")
            .WithMessage("Некорректный формат Password.");
    }
}