using DACN.DTOs.Request;
using Microsoft.AspNetCore.SignalR;

namespace DACN.Service.Hubs
{
    public class Notifications : Hub
    {
        public async Task SendNotifications(int userAccountId, SendNotificationRequest dto)
        {
            await Clients.User(userAccountId.ToString())
                         .SendAsync("NotificationEmployee", dto);
        }


    }
}
