using UserService.Domain.Entities;

namespace UserService.Domain.Repos;

public interface IUserRepository
{
    Task<List<User>> GetAllUsersReadonlyAsync(
        Guid currentUserId,
        string? teamName,
        bool isCurrentUserIncluded,
        CancellationToken ct = default);

    Task<User?> GetUserReadonlyByIdAsync(Guid id, CancellationToken ct = default);

    Task<Guid?> GetUserIdByEmailAsync(string email, CancellationToken ct = default);

    Task<List<User>> GetSubordinatesReadonlyByUserIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> AreUsersExistAsync(List<Guid> userIds, CancellationToken ct = default);

    Task<List<User>> GetUsersByIdsReadonlyAsync(List<Guid> userIds, CancellationToken ct = default);
}