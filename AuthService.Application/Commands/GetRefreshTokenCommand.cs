using AuthService.Application.interfaces;
using AuthService.Application.Models;
using AuthService.Domain.Repos;
using Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Commands;

public sealed record GetRefreshTokenCommand(string RefreshToken) : IRequest<Result<TokenResponse>>;

public sealed class GetRefreshTokenCommandHandler : IRequestHandler<GetRefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IAuthRepository _authRepository;

    public GetRefreshTokenCommandHandler(IJwtProvider jwtProvider, IAuthRepository authRepository)
    {
        _jwtProvider = jwtProvider;
        _authRepository = authRepository;
    }

    public async Task<Result<TokenResponse>> Handle(GetRefreshTokenCommand request, CancellationToken cancellationToken)
    {
        
        var user = await _authRepository.GetUserByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user is null)
            return Result<TokenResponse>.Failure(Error.Unauthorized("Общая ошибка"));
        if (user.RefreshTokenExpiryTime < DateTimeOffset.UtcNow)
            return Result<TokenResponse>.Failure(Error.Unauthorized("Токен истек"));
        
        var accessToken = _jwtProvider.GenerateAccessToken(user);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTimeOffset.UtcNow.AddHours(_jwtProvider.GetRefreshExpiresHours());

        await _authRepository.UpdateUserAsync(user, cancellationToken);

        var tokenResponse = new TokenResponse()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };

        return Result<TokenResponse>.Success(tokenResponse);
    }
}

public class GetRefreshTokenCommandValidator : AbstractValidator<GetRefreshTokenCommand>
{
    public GetRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotNull().WithMessage("Токен необходим")
            .NotEmpty().WithMessage("Токен не должен быть пустым");
    }
}