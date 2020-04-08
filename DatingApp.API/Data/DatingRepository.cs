using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context) => _context = context;

        public void Add<T>(T entity) where T : class => _context.Add(entity);

        public void Delete<T>(T entity) where T : class => _context.Remove(entity);

        public async Task<User> GetUser(int id) => await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == id);

        public async Task<Photo> GetPhoto(int id) => await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

        public async Task<PagedList<User>> GetUsers(UserParams userParams) 
        {
            var qry = _context.Users.Include(u => u.Photos).Where(u => u.Id != userParams.UserId && u.Gender == userParams.Gender).AsQueryable();
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Now.AddYears(-userParams.MaxAge-1);
                var maxDob = DateTime.Now.AddYears(-userParams.MinAge);
                qry = qry.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            }
            switch (userParams.OrderBy)
            {
                case "created":
                    qry = qry.OrderByDescending(u => u.Created);
                    break;
                default:
                    qry = qry.OrderByDescending(u => u.LastActive);
                    break;
                    
            }
            return await PagedList<User>.CreateAsync(
                qry,
                userParams.PageNumber,
                userParams.PageSize
            );
        } 

        public async Task<bool> SaveAll() => await _context.SaveChangesAsync() > 0;

        public async Task<Photo> GetMainPhotoForUser(int userId) => await _context.Photos.FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);
    }
}