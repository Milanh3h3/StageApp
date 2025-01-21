using Microsoft.AspNetCore.SignalR;

namespace StageApp.Hubs
{
    public class FeedbackHub : Hub
    {
        public async Task SendFeedback(string message)
        {
            await Clients.All.SendAsync("ReceiveFeedback", message);
        }
    }
}