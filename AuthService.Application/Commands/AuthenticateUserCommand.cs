using AuthService.Application.interfaces;
using AuthService.Application.ServiceClientsAbstract;
using AuthService.Application.Models;
using AuthService.Domain.Repos;
using MediatR;
using Common;
using FluentValidation;

namespace AuthService.Application.Commands;

public sealed record AuthenticateUserCommand(string Email, string Password) : IRequest<Result<LoginResponseModel>>;

public sealed class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, Result<LoginResponseModel>>
{
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthRepository _authRepository;
    private readonly IUserServiceClient _userServiceClient;

    public AuthenticateUserCommandHandler(
        IJwtProvider jwtProvider,
        IPasswordHasher passwordHasher,
        IAuthRepository authRepository,
        IUserServiceClient userServiceClient)
    {
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
        _authRepository = authRepository;
        _userServiceClient = userServiceClient;
    }

    public async Task<Result<LoginResponseModel>> Handle(AuthenticateUserCommand request,
        CancellationToken ct = default)
    {
        var user = await _authRepository.GetUserByEmailReadOnlyAsync(request.Email, ct);
        if (user is null)
            return Error.NotFound("Не удалось войти в систему");

        var result = _passwordHasher.IsPasswordValid(request.Password, user.PasswordHash);
        if (!result)
            return Error.Unauthorized("Не удалось войти в систему");

        var userFromUserService = await _userServiceClient.GetUserByIdAsync(user.UserId, ct);
        if (userFromUserService is null)
            return Error.Unauthorized("Пользователь не найден в UserService");

        var accessToken = _jwtProvider.GenerateAccessToken(user, userFromUserService.Position);
        var refreshToken = _jwtProvider.GenerateRefreshToken();

        user.SetRefreshToken(
            refreshToken,
            DateTimeOffset.UtcNow.AddHours(_jwtProvider.GetRefreshExpiresHours())
        );

        await _authRepository.UpdateRefreshTokenUserAsync(user, ct);

        var tokens = TokensModel.Create(accessToken, refreshToken);

        var loginResponse = LoginResponseModel.Create(user.UserId, tokens);

        return loginResponse;
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