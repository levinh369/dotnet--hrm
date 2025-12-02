using DACN.DTOs.Respone;
using DACN.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DACN.Areas.Employee.Controllers
{
    [Area("Employee")]
    public class NotificationController : Controller
    {
        private readonly NotificationRepository notificationRepository;
        public NotificationController(NotificationRepository notificationRepository)
        {
            this.notificationRepository = notificationRepository;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> loadNotification(int page = 1, int pageSize = 6)
        {
            var userAccountIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userAccountIdString, out int userAccountId))
            {
                return Json(new { success = false, message = "Không xác định được UserAccountId" });
            }
            try
            {
                var (entities, total) = await notificationRepository.GetNotificationAsync(page, pageSize, userAccountId);
                var data = entities.Select(n => new NotificationRespone
                {
                   NotificationId= n.NotificationId,
                   Title= n.Title,
                   Content= n.Content,
                   CreatedAt= n.CreatedAt,
                   IsRead= n.IsRead,
                   Type= n.Type,
                   Url= n.Url
                }).ToList();
                return Json(data);
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tải thông báo", error = ex.Message });
            }
        }
    }
}
