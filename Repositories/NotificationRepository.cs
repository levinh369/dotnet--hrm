using DACN.Data;
using DACN.DTOs.Request;
using DACN.Models;
using Microsoft.EntityFrameworkCore;

namespace DACN.Repositories
{
    public class NotificationRepository
    {
        private readonly ApplicationDbContext db;

        public NotificationRepository(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<int> GetUnreadCountAsync(int employeeId)
        {
            return await db.Notifications
                .Where(n => n.EmployeeId == employeeId && !n.IsRead)
                .CountAsync();
        }
        public async Task<(List<NotificationModel> data, int total)> GetNotificationAsync(int page, int pageSize, int userAccountId)
        {
            var query = db.Notifications
                          //.Include(n => n.JobApplication) // nếu có navigation
                          .Where(n => n.UserId == userAccountId)
                          .OrderByDescending(n => n.CreatedAt);

            int total = await query.CountAsync();

            var data = await query
                          .Skip((page - 1) * pageSize)
                          .Take(pageSize)
                          .ToListAsync();

            return (data, total);
        }
        public async Task CreateNotificationAsync(SendNotificationRequest dto)
        {
            var notif = new NotificationModel
            {
                EmployeeId = dto.EmployeeId,
                Title = dto.Title,
                Content = dto.Content,
                Type = dto.Type,
                Url = dto.Url ?? string.Empty,
                CreatedAt = DateTime.Now,
                IsRead = false,
                UserId = dto.UserAccountId
            };
            db.Notifications.Add(notif);
            await db.SaveChangesAsync();
        }

    }
}
