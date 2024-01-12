using Microsoft.AspNetCore.SignalR;
using socialmediaAPI.Models.Entities;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.Services.CloudinaryService;
using System.Text.Json;

namespace socialmediaAPI.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("user connected");
            var httpContext = Context.GetHttpContext();
            var userId = httpContext?.Request.Cookies["userID"];
            await Groups.AddToGroupAsync(Context.ConnectionId, userId ?? "null");
            await Clients.All.SendAsync("UserConnected", Context.ConnectionId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContext = Context.GetHttpContext();
            var userId = httpContext?.Request.Cookies["userID"];
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId ?? "null");

            // Notify clients that a user disconnected
            await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(List<string> receiverIds, string conversationId, Message message)
        {
            Console.WriteLine(JsonSerializer.Serialize(message));
            await Clients.All.SendAsync("ReceiveMessage", receiverIds, conversationId, message);
            await Clients.Caller.SendAsync("MessageStatus", conversationId, message);
        }

        public async Task DeleteMessage(List<string> receiverIds,string conversationId, string messageId)
        {
            Console.WriteLine($"data is: ${receiverIds},${conversationId}");
            await Clients.All.SendAsync("DeleteMessage", receiverIds, conversationId, messageId);
            await Clients.Caller.SendAsync("MessageStatus", conversationId, messageId);
        }

    }
}
//public async Task ReplaceMessage(List<string> receiversIds, string conversationId, string message)
//{
//    await Clients.Groups(receiversIds).SendAsync("UpdateMessage", conversationId, message);
//    await Clients.Caller.SendAsync("MessageStatus", "Success: update Success", conversationId, message);
//}