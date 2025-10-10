using AuthService.Application.interfaces;
using AuthService.Application.Models;
using AuthService.Domain.Entities;
using AuthService.Domain.Repos;
using MediatR;
using Common;
using FluentValidation;

namespace AuthService.Application.Commands;

public sealed record AuthenticateUserCommand(string Email,  string Password) : IRequest<Result<TokenResponse>>;

public sealed class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, Result<TokenResponse>>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthRepository _authRepository;
    
    public AuthenticateUserCommandHandler(IJwtProvider jwtProvider, IPasswordHasher passwordHasher, IAuthRepository authRepository)
    {
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
        _authRepository = authRepository;
    }
    
    public async Task<Result<TokenResponse>> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken =  default)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return Result<TokenResponse>.Failure(Error.Unauthorized("Failed to login"));
        
        var result = _passwordHasher.VerifyHashedPassword(request.Password, user.PasswordHash);
        if (!result)
            return Result<TokenResponse>.Failure(Error.Unauthorized("Failed to login"));
        
        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken();
        
        user.RefreshToken =  refreshToken;
        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddHours(_jwtProvider.GetRefreshExpiresHouse());

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
            .NotNull().NotEmpty()
            .EmailAddress().WithMessage("Некорректный формат email.")
            .MaximumLength(255).WithMessage("email должен быть не более 255 символов.");
        RuleFor(x => x.Password)
            .NotNull()
            .MinimumLength(4).WithMessage("Пароль должен быть не менее 4 символов.")
            .MaximumLength(512).WithMessage("Пароль должен быть не более 100 символов.")
            .Matches(@"^(?!.*\s)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s]).+$");
    }
}