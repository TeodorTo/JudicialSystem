using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

public class CallHub : Hub
{
    private static readonly ConcurrentDictionary<string, (string Room, string UserName)> UserRooms = new();
    private static readonly ConcurrentDictionary<string, bool> UserMuteStatus = new();

    public async Task JoinRoom(string roomName, string userName)
    {
        UserRooms[Context.ConnectionId] = (roomName, userName);
        UserMuteStatus[Context.ConnectionId] = false; // mute = false по подразбиране

        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);

        var usersInRoom = UserRooms
            .Where(kvp => kvp.Value.Room == roomName && kvp.Key != Context.ConnectionId)
            .Select(kvp => new {
                ConnectionId = kvp.Key,
                UserName = kvp.Value.UserName,
                IsMuted = UserMuteStatus.TryGetValue(kvp.Key, out var muted) && muted
            })
            .ToList();

        await Clients.Client(Context.ConnectionId).SendAsync("ExistingUsers", usersInRoom);
        await Clients.Group(roomName).SendAsync("UserJoined", Context.ConnectionId, userName);
    }


    public async Task SendSignalToRoom(string targetConnectionId, string signalData)
    {
        await Clients.Client(targetConnectionId).SendAsync("ReceiveSignal", Context.ConnectionId, signalData);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        if (UserRooms.TryRemove(Context.ConnectionId, out var userInfo))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userInfo.Room);
            await Clients.Group(userInfo.Room).SendAsync("UserLeft", Context.ConnectionId);
        }

        UserMuteStatus.TryRemove(Context.ConnectionId, out _);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task ToggleMute(bool isMuted)
    {
        if (UserRooms.TryGetValue(Context.ConnectionId, out var userInfo))
        {
            UserMuteStatus[Context.ConnectionId] = isMuted;
            await Clients.Group(userInfo.Room).SendAsync("UserMuteToggled", Context.ConnectionId, isMuted);
        }
    }


}