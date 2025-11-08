using Common;
using FluentValidation;
using MediatR;
using UserService.Domain.Repos;

namespace UserService.Application.Queries;

public sealed record AreUsersExistQuery(List<Guid> UserIds) : IRequest<Result<bool>>;

public sealed class AreUsersExistQueryHandler : IRequestHandler<AreUsersExistQuery, Result<bool>>
{
    private readonly IUserRepository _repo;

    public AreUsersExistQueryHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<bool>> Handle(AreUsersExistQuery request, CancellationToken ct)
    {
        var exist = await _repo.AreUsersExistAsync(request.UserIds, ct);
        return Result<bool>.Success(exist);
    }
}

// public sealed class AreUsersExistQueryValidator : AbstractValidator<AreUsersExistQuery>
// {
//     public AreUsersExistQueryValidator()
//     {
//         RuleFor(x => x.UserIds)
//             .NotNull().WithMessage("Список идентификаторов не должен быть null.")
//             .Must(ids => ids.All(id => id != Guid.Empty))
//             .WithMessage("Список не должен содержать пустых идентификаторов.");
//     }
// }
