using Microsoft.EntityFrameworkCore;
using AuthService.Domain.Entities;
using AuthService.Domain.Repos;
using AuthService.Infrastructure.Db;

namespace AuthService.Infrastructure;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _dbContext;

    public AuthRepository(AuthDbContext authDbContext)
    {
        _dbContext = authDbContext;
    }
    
    public async Task CreateRegistrationAsync(User user, CancellationToken ct = default)
    {
        await _dbContext.Users.AddAsync(user, ct);
        await _dbContext.SaveChangesAsync(ct);
    }
    
    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task UpdateUserAsync(User user, CancellationToken ct = default)
    {
        _dbContext.Users
            .Update(user);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Userid == userId, ct);
    }
}