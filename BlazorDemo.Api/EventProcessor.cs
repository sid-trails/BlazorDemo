using Microsoft.AspNetCore.SignalR;

namespace BlazorDemo.Api;

public class EventProcessor: BackgroundService
{
    private static readonly TimeSpan period = TimeSpan.FromSeconds(5);
    private readonly ILogger<EventProcessor> _logger;
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    
    public EventProcessor(ILogger<EventProcessor> logger, IHubContext<NotificationHub, INotificationClient> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }
    
    protected override  async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(period);
        while (!stoppingToken.IsCancellationRequested)
        {
            await _hubContext.Clients.All.ReceiveNotification($"Event has been processed {DateTime.Now}");
        }
    }
}
/*
 *using System;
   using Azure.Messaging.EventHubs;
   using Azure.Messaging.EventHubs.Consumer;
   using Azure.Messaging.EventHubs.Processor;
   using Azure.Storage.Blobs;
   
   // ...
   
  
   const string eventHubName = "<your_event_hub_name>";
   const string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
   var storageConnectionString = "<your_storage_connection_string>";
   var storageContainerName = "<your_storage_container_name>";
   
   var storageClient = new BlobContainerClient(
       "<AZURE_STORAGE_CONNECTION_STRING>", "<BLOB_CONTAINER_NAME>");
   var eventProcessorClient = new EventProcessorClient(
       storageClient,
       consumerGroup,
       connectionString,
       eventHubName);
   
   eventProcessorClient.ProcessEventAsync += ProcessEventHandler;
   eventProcessorClient.ProcessErrorAsync += ProcessErrorHandler;
   
   await eventProcessorClient.StartProcessingAsync();
   
   // ...
   
   await eventProcessorClient.StopProcessingAsync();
   
   // ...
   
   static Task ProcessEventHandler(ProcessEventArgs eventArgs)
   {
       // Process the received event
       Console.WriteLine($"Received event: {eventArgs.Data.EventBody}");
   
       // Return a completed task
       return Task.CompletedTask;
   }
   
   static Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
   {
       // Handle the error
       Console.WriteLine($"Error processing event: {eventArgs.Exception}");
   
       // Return a completed task
       return Task.CompletedTask;
   }
 *
 * 
 */