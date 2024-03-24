using Microsoft.AspNetCore.SignalR;

namespace BlazorDemo.Api;

public class NotificationHub:Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        await Clients.Client(Context.ConnectionId).ReceiveNotification("Connection to the server has been established");
        
    }
}

public interface INotificationClient
{
    Task ReceiveNotification(string message);
}