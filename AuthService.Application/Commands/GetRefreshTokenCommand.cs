using AuthService.Application.interfaces;
using AuthService.Application.Models;
using AuthService.Application.ServiceClientsAbstract;
using AuthService.Domain.Repos;
using Common;
using FluentValidation;
using MediatR;

namespace AuthService.Application.Commands;

public sealed record GetRefreshTokenCommand : IRequest<Result<TokensModel>>
{
    public required string RefreshToken { get; init; }
}

public sealed class GetRefreshTokenCommandHandler : IRequestHandler<GetRefreshTokenCommand, Result<TokensModel>>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IAuthRepository _authRepository;
    private readonly IUserServiceClient _userServiceClient;

    public GetRefreshTokenCommandHandler(
        IJwtProvider jwtProvider, 
        IAuthRepository authRepository,
        IUserServiceClient userServiceClient)
    {
        _jwtProvider = jwtProvider;
        _authRepository = authRepository;
        _userServiceClient = userServiceClient;
    }

    public async Task<Result<TokensModel>> Handle(GetRefreshTokenCommand request, CancellationToken ct)
    {
        var user = await _authRepository.GetUserByRefreshTokenReadOnlyAsync(request.RefreshToken, ct);
        if (user is null)
            return Result<TokensModel>.Failure(Error.Unauthorized("Ошибка авторизации"));
        if (user.RefreshTokenExpiryTime < DateTimeOffset.UtcNow)
            return Result<TokensModel>.Failure(Error.Unauthorized("Токен истек"));

        var userFromUserService = await _userServiceClient.GetUserByIdAsync(user.UserId, ct);
        if (userFromUserService is null)
            return Result<TokensModel>.Failure(Error.Unauthorized("Пользователь не найден в UserService"));
        
        var accessToken = _jwtProvider.GenerateAccessToken(user, userFromUserService.Position);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        user.SetRefreshToken(
            refreshToken,
            DateTimeOffset.UtcNow.AddHours(_jwtProvider.GetRefreshExpiresHours())
        );

        await _authRepository.UpdateRefreshTokenUserAsync(user, ct);

        var tokenResponse = new TokensModel()
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };

        return Result<TokensModel>.Success(tokenResponse);
    }
}

public class GetRefreshTokenCommandValidator : AbstractValidator<GetRefreshTokenCommand>
{
    public GetRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotNull().WithMessage("Не передан RefreshToken")
            .NotEmpty().WithMessage("Передан пустой RefreshToken");
    }
}