using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace PersonaXFleet.Models
{
    public class NotificationHub : Hub
    {
        public async Task JoinNotificationGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }
}
