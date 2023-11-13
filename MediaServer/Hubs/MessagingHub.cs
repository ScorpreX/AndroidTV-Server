using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace MediaServer.Hubs;

public class MessagingHub : Hub
{
    static ConcurrentDictionary<string, string> users = new ConcurrentDictionary<string, string>();

    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }

    public async Task Connect(string username)
    {
        var connectionId = Context.ConnectionId;
        users[username] = connectionId;
        await Clients.Caller.SendAsync("Connected", "You are connected!");
    }
}

