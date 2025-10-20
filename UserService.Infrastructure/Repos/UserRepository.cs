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
}