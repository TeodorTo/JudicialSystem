using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace Judicial_system.Hubs;



public class CallHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> UserRooms = new();

    public async Task JoinRoom(string roomName)
    {
        UserRooms[Context.ConnectionId] = roomName;
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("UserJoined", Context.ConnectionId);
    }

    public async Task SendSignalToRoom(string targetConnectionId, string signalData)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", Context.ConnectionId, signalData);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (UserRooms.TryRemove(Context.ConnectionId, out var roomName))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            await Clients.Group(roomName).SendAsync("UserLeft", Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }
}