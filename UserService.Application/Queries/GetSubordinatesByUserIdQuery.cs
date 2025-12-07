using Common;
using MediatR;
using UserService.Application.Models;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record GetSubordinatesByUserIdQuery(Guid Id) : IRequest<Result<SubordinatesModel>>;

internal sealed class GetSubordinatesByUserIdQueryHandler : IRequestHandler<GetSubordinatesByUserIdQuery, Result<SubordinatesModel>>
{
    private readonly IUserRepository _repo;

    public GetSubordinatesByUserIdQueryHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<SubordinatesModel>> Handle(GetSubordinatesByUserIdQuery request, CancellationToken ct)
    {
        if (await _repo.GetUserReadonlyByIdAsync(request.Id, ct) is null)
        {
            return Result<SubordinatesModel>.Failure(Error.NotFound($"Пользователь с id {request.Id} не найден."));
        }

        var subordinates = await _repo.GetSubordinatesReadonlyByUserIdAsync(request.Id, ct);

        return Result<SubordinatesModel>.Success(
            new SubordinatesModel(
                subordinates
                    .Select(s => s.ToAppModel())
                    .ToList()));
    }
}