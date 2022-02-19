using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebApplication.SignalRooms
{
    //Class inherited from SignalR Hub
    public class SignalRoom : Hub
    {
        public async Task SendMessage(string connectionID, string userName, string message)
        {
            await Clients.AllExcept(connectionID).SendAsync("ReceivedMessage", userName, message);
        }
    }
}
