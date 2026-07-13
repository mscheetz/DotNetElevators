using Microsoft.AspNetCore.SignalR;

namespace DotNetElevators;

public sealed class BuildingHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine("Client connected: {0}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
        Console.WriteLine("Client disconnected: {0}", Context.ConnectionId);
        await base.OnDisconnectedAsync(ex);
    }
}