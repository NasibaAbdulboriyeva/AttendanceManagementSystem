using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace AttendanceManagementSystem.Infrastructure.Persistence.Repositories
{
    namespace AttendanceManagementSystem.Application.Abstractions
    {
        public class RefreshTokenRepository : IRefreshTokenRepository
        {
            private readonly AppDbContext _context;

            public RefreshTokenRepository(AppDbContext myDbContext)
            {
                _context = myDbContext;
            }
            public async Task InsertRefreshTokenAsync(RefreshToken refreshToken)
            {
                await _context.RefreshTokens.AddAsync(refreshToken);
                await _context.SaveChangesAsync();
            }
            public async Task RemoveRefreshTokenAsync(string token)
            {
                var rToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == token);
                if (rToken == null)
                {
                    throw new KeyNotFoundException($"Refresh token {token} not found.");
                }
                _context.RefreshTokens.Remove(rToken);
                await _context.SaveChangesAsync();
            }
            public async Task<RefreshToken> SelectRefreshTokenAsync(string refreshToken, long userId)
            {
                var token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == userId);
                if (token == null)
                {
                    throw new KeyNotFoundException($"Refresh token {refreshToken} for user ID {userId} not found.");
                }
                return token;
            }

            public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
            {
                var existingToken = await _context.RefreshTokens
                   .FirstOrDefaultAsync(t => t.Token == refreshToken.Token && t.UserId == refreshToken.UserId);
                if (existingToken == null)
                {
                    throw new Exception($"Refresh token {refreshToken.Token} not found for user {refreshToken.UserId}");
                }
                existingToken.IsRevoked = refreshToken.IsRevoked;

                existingToken.Expires = refreshToken.Expires;

                _context.RefreshTokens.Update(existingToken);
                await _context.SaveChangesAsync();
            }
        }
    }

}
