using Common;
using MediatR;
using RecommendationService.Application.Models;
using RecommendationService.Application.ServiceClientsAbstract;

namespace RecommendationService.Application.Queries;

public sealed record GetUserInfoForCheckAccessQuery(Guid UserId) : IRequest<Result<ShortUserModel>>;

public sealed class GetUserInfoForCheckAccessQueryHandler(
    IUserServiceClient userServiceClient)
    : IRequestHandler<GetUserInfoForCheckAccessQuery, Result<ShortUserModel>>
{
    public async Task<Result<ShortUserModel>> Handle(GetUserInfoForCheckAccessQuery request, CancellationToken ct)
    {
        var user = await userServiceClient.GetUserByIdAsync(request.UserId, ct);
        return user == null
            ? Error.NotFound("Пользователь не был найден")
            : user;
    }
}