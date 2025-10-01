using MediatR;
using UserService.Application.Dto;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record GetSubordinatesByUserIdQuery(Guid Id) : IRequest<Result<SubordinatesDto>>;

public sealed class GetSubordinatesByUserIdQueryHandler : IRequestHandler<GetSubordinatesByUserIdQuery, Result<SubordinatesDto>>
{
    private readonly IUserRepository _repo;

    public GetSubordinatesByUserIdQueryHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<SubordinatesDto>> Handle(GetSubordinatesByUserIdQuery request, CancellationToken ct)
    {
        var subordinates = await _repo.GetSubordinatesByUserIdAsync(request.Id, ct);

        //TODO: Обработка и разделени ошибок (SP-24)
        return Result<SubordinatesDto>.Success(
            new SubordinatesDto(
                subordinates.Select(s => s.ToDto())
                    .ToList())
        );
    }
}