using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        
        public AuthRepository(DataContext context) => _context = context;

        public async Task<User> Login(string username, string password)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.UserName == username);
            return (user == null/* || !VerifyPasswordHash(password, user)*/) ? null : user;
        }

        public async Task<User> Register(User user, string password)
        {
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            
            // user.PasswordHash = passwordHash;
            // user.PasswordSalt = passwordSalt;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UserExists(string username) => await _context.Users.AnyAsync(u => u.UserName == username);

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, User user)
        {
            // using(var hmac = new System.Security.Cryptography.HMACSHA512(user.PasswordSalt))
            // {
            //     var comutedHash =  hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            //     for (int i=0; i<comutedHash.Length; i++) if (comutedHash[i] != user.PasswordHash[i]) return false;
            // }
            return true;
        }
    }
}