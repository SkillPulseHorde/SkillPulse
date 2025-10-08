using MediatR;
using UserService.Application;

namespace AuthService.Application.Commands.Register;

public record RegisterCommand (string Email, string Password) : IRequest<Result<>>;
{
    
}