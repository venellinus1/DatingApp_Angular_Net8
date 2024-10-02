using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

[Authorize]
public class PresenceHub(PresenceTracker presenceTracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (Context.User == null) throw new HubException("Cannot get current user claim");// otherwise get a warning around Context.User.GetUsername()

        await presenceTracker.UserConnected(Context.User.GetUsername(), Context.ConnectionId);
        await Clients.Others.SendAsync("UserIsOnline", Context.User?.GetUsername());

        var currentUsers = await presenceTracker.GetOnlineUsers();
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.User == null) throw new HubException("Cannot get current user claim");// otherwise get a warning around Context.User.GetUsername()

        await presenceTracker.UserDisconnected(Context.User.GetUsername(), Context.ConnectionId);
        await Clients.Others.SendAsync("UserIsOffline", Context.User?.GetUsername());

        //now get the users who are still connected
        var currentUsers = await presenceTracker.GetOnlineUsers();
        await Clients.All.SendAsync("GetOnlineUsers", currentUsers);

        await base.OnDisconnectedAsync(exception);
    }
}