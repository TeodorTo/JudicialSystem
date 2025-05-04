using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Judicial_system.Hubs;

public class ChatHub : Hub
{
    // Потребителско име -> ConnectionId
    private static readonly ConcurrentDictionary<string, string> Users = new();

    public override Task OnConnectedAsync()
    {
        string username = Context.User.Identity?.Name ?? Context.ConnectionId;

        Users[username] = Context.ConnectionId;

        Clients.All.SendAsync("UserListUpdated", Users.Keys.ToList());

        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var user = Users.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (!string.IsNullOrEmpty(user.Key))
        {
            Users.TryRemove(user.Key, out _);
            Clients.All.SendAsync("UserListUpdated", Users.Keys.ToList());
        }

        return base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string user, string message)
    {
        // Все още се поддържа публично съобщение
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task SendPrivateMessage(string toUser, string message)
    {
        string fromUser = Context.User.Identity?.Name ?? "Unknown";

        if (Users.TryGetValue(toUser, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceivePrivateMessage", fromUser, message);
        }
    }
}