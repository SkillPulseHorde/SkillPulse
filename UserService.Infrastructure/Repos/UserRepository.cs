using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repos;
using UserService.Infrastructure.Db;

namespace UserService.Infrastructure.Repos;

public sealed class UserRepository : IUserRepository
{
    private readonly UserDbContext _dbContext;

    public UserRepository(UserDbContext userDbContext)
    {
        _dbContext = userDbContext;
    }

    public async Task<List<User>> GetAllUsersReadonlyAsync(Guid currentUserId,
        string? teamName,
        bool isCurrentUserIncluded,
        CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => isCurrentUserIncluded || u.Id != currentUserId)
            .OrderByDescending(u => u.TeamName == teamName)
            .ThenBy(u => u.TeamName == null)
            .ThenBy(u => u.TeamName)
            .ThenBy(u => u.LastName)
            .ToListAsync(ct);
    }

    public async Task<User?> GetUserReadonlyByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Manager)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<Guid?> GetUserIdByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .Select(u => (Guid?)u.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<User>> GetSubordinatesReadonlyByUserIdAsync(Guid managerId, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .Where(u => u.ManagerId == managerId)
            .AsNoTracking()
            .ToListAsync(ct);
    }
    
    public async Task<bool> AreUsersExistAsync(List<Guid> userIds, CancellationToken ct = default)
    {
        if (userIds.Count == 0)
            return true;
        
        var foundCount = await _dbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => u.Id)
            .Distinct()
            .CountAsync(ct);

        return foundCount == userIds.Count;
    }
    
    public async Task<List<User>> GetUsersByIdsReadonlyAsync(List<Guid> userIds, CancellationToken ct = default)
    {
        if (userIds.Count == 0)
            return [];
        
        return await _dbContext.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync(ct);
    }
}