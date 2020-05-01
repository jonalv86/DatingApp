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
            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                qry = qry.Where(u => userLikers.Contains(u.Id));
            }
            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                qry = qry.Where(u => userLikees.Contains(u.Id));
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
            return await PagedList<User>.CreateAsync(qry, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll() => await _context.SaveChangesAsync() > 0;

        public async Task<Photo> GetMainPhotoForUser(int userId) => await _context.Photos.FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);

        public async Task<Like> GetLike(int userId, int recipientId) => await _context.Likes.FirstOrDefaultAsync(l => l.LikerId == userId && l.LikeeId == recipientId);

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.Include(u => u.Likers).Include(u => u.Likees).FirstOrDefaultAsync(u => u.Id == id);
            return likers ? user.Likers.Where(u => u.LikeeId == id).Select(u => u.LikerId) : user.Likees.Where(u => u.LikerId == id).Select(u => u.LikeeId);
        }

        public async Task<Message> GetMessage(int id) => await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var qry = _context.Messages.Include(m => m.Sender).ThenInclude(u => u.Photos).Include(m => m.Recipient).ThenInclude(u => u.Photos).AsQueryable();
            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    qry = qry.Where(m => m.RecipientId == messageParams.UserId && !m.RecipientDeleted);
                    break;
                case "Outbox":
                    qry = qry.Where(m => m.SenderId == messageParams.UserId && !m.SenderDeleted);
                    break;
                default:
                    qry = qry.Where(m => m.RecipientId == messageParams.UserId && !m.RecipientDeleted && !m.IsRead);
                    break;
            }
            qry = qry.OrderByDescending(m => m.MessageSent);
            return await PagedList<Message>.CreateAsync(qry, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId) => 
            await _context.Messages.Include(m => m.Sender).ThenInclude(u => u.Photos).Include(m => m.Recipient).ThenInclude(u => u.Photos)
                .Where(m => m.RecipientId == userId && !m.RecipientDeleted && m.SenderId == recipientId || 
                       m.RecipientId == recipientId && !m.SenderDeleted && m.SenderId == userId)
                .OrderByDescending(m => m.MessageSent).ToListAsync();
    }
}