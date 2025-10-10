using AuthService.Application.interfaces;
using AuthService.Application.Models;
using AuthService.Domain.Entities;
using AuthService.Domain.Repos;
using Common;
using MediatR;
using FluentValidation;

namespace AuthService.Application.Commands;

public sealed record CreateRegistrationCommand (string Email, string Password) : IRequest<Result<string>>;

public sealed class CreateRegistrationCommandHandler : IRequestHandler<CreateRegistrationCommand, Result<string>>
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthRepository _authRepository;
    
    public CreateRegistrationCommandHandler(IPasswordHasher passwordHasher, IAuthRepository authRepository)
    {
        _passwordHasher = passwordHasher;
        _authRepository = authRepository;
    }

    public async Task<Result<string>> Handle(CreateRegistrationCommand request, CancellationToken ct = default)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email, ct);
        if (user is not null)
            return Result<string>.Failure(Error.Conflict("email is already in use"));
        
        var hashedPassword = _passwordHasher.GeneratePasswordHash(request.Password);
        
        var newUser = new User
        {
            Userid = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = hashedPassword
        };
        
        await _authRepository.PostRegistrationAsync(newUser, ct);
        
        return Result<string>.Success("");
    }
}

public class CreateRegistrationCommandValidator : AbstractValidator<CreateRegistrationCommand>
{
    public CreateRegistrationCommandValidator()
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