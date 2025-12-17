using AttendanceManagementSystem.Application.Abstractions;
using AttendanceManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagementSystem.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
    
        public async Task DeleteUserAsync(long userId)
        {
            var user = await SelectUserByIdAsync(userId);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<long> InsertUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user.UserId;
        }

        public async Task<ICollection<User>> SelectAllUsersAsync(int skip, int take)
        {
            return await _context.Users
                 .Skip(skip)
                 .Take(take)
                 .ToListAsync();
        }

        public async Task<User> SelectUserByIdAsync(long id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }
            return user;
        }

        public async Task<User?> SelectUserByUserNameAsync(string userName)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task UpdateUserRoleAsync(long userId, UserRole userRole)
        {
            var user = await SelectUserByIdAsync(userId);
            _context.Users.Update(user);

        }
    }
}