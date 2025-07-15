using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace PersonaXFleet.Models
{
    public class NotificationHub : Hub
    {
        public async Task JoinGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
    }


}
