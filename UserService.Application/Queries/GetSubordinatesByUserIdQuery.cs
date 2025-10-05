
using Common;
using MediatR;
using UserService.Application.Dto;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record GetSubordinatesByUserIdQuery(Guid Id) : IRequest<Result<SubordinatesDto>>;

internal sealed class GetSubordinatesByUserIdQueryHandler : IRequestHandler<GetSubordinatesByUserIdQuery, Result<SubordinatesDto>>
{
    private readonly IUserRepository _repo;

    public GetSubordinatesByUserIdQueryHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<SubordinatesDto>> Handle(GetSubordinatesByUserIdQuery request, CancellationToken ct)
    {
        if (await _repo.GetUserByIdAsync(request.Id, ct) is null)
        {
            return Result<SubordinatesDto>.Failure(Error.NotFound($"User with id {request.Id} not found."));
        }
        
        var subordinates = await _repo.GetSubordinatesByUserIdAsync(request.Id, ct);
        
        return Result<SubordinatesDto>.Success(
            new SubordinatesDto(
                subordinates
                    .Select(s => s.ToDto())
                    .ToList()));
    }
}