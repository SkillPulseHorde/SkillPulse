using AuthService.Api.DTO;
using AuthService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Endpoints;

public static class CreateRegistration
{
    public static IEndpointRouteBuilder MapCreateRegistrationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (
                    [FromBody] RegistrationRequest request,
                    IMediator mediator,
                    CancellationToken ct) =>
                {
                    var command = new CreateRegistrationCommand(request.Email, request.Password);
                    var result = await mediator.Send(command, ct);

                    return result.IsSuccess
                        ? Results.Ok()
                        : result.Error!.ToProblemDetails();
                })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .AllowAnonymous()
            .WithSummary("Зарегистрировать пользователя")
            .WithDescription("Возвращает только статус-код");

        return app;
    }
}