using AuthService.Application.interfaces;
using AuthService.Application.ServiceClientsAbstract;
using AuthService.Domain.Entities;
using AuthService.Domain.Repos;
using Common;
using MediatR;
using FluentValidation;

namespace AuthService.Application.Commands;

public sealed record CreateRegistrationCommand(string Email, string Password) : IRequest<Result>;

public sealed class CreateRegistrationCommandHandler : IRequestHandler<CreateRegistrationCommand, Result>
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthRepository _authRepository;
    private readonly IUserServiceClient _userServiceClient;

    public CreateRegistrationCommandHandler(
        IPasswordHasher passwordHasher,
        IAuthRepository authRepository,
        IUserServiceClient userServiceClient)
    {
        _passwordHasher = passwordHasher;
        _authRepository = authRepository;
        _userServiceClient = userServiceClient;
    }

    public async Task<Result> Handle(CreateRegistrationCommand request, CancellationToken ct = default)
    {
        var user = await _authRepository.GetUserByEmailReadOnlyAsync(request.Email, ct);
        if (user is not null)
            return Result.Failure(Error.Conflict("Эта почта уже используется"));

        var userIdFromUserService = await _userServiceClient.GetUserIdByEmailAsync(request.Email, ct);
        if (userIdFromUserService is null)
            return Result.Failure(Error.NotFound("Пользователя с такой почтой нет в системе"));

        var hashedPassword = _passwordHasher.GeneratePasswordHash(request.Password);

        var newUser = new User
        {
            UserId = userIdFromUserService.Value,
            Email = request.Email,
            PasswordHash = hashedPassword
        };

        await _authRepository.CreateRegistrationAsync(newUser, ct);

        return Result.Success();
    }
}

public class CreateRegistrationCommandValidator : AbstractValidator<CreateRegistrationCommand>
{
    public CreateRegistrationCommandValidator()
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