using Microsoft.AspNetCore.SignalR;
using ServiceReference;
using System;
using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using BlazorDemo.Api.Controllers;

namespace BlazorDemo.Api;

public class EventProcessor: BackgroundService
{
    private static readonly TimeSpan period = TimeSpan.FromSeconds(10);
    private readonly ILogger<EventProcessor> _logger;
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ITaskService _taskService;
    
    public EventProcessor(ILogger<EventProcessor> logger,ITaskService taskService ,IHubContext<NotificationHub, INotificationClient> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
        _taskService = taskService;
    }
    
    protected override  async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(period);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        
        {
            const string connectionString = "Endpoint=sb://task-service.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hlysiD2Hfkt9yZW0Y9WqHWI5VnrAF88Je+AEhCoOHGo=";
            const string eventHubName = "jag-test";
            const string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            const string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=eventclientprocessor;AccountKey=pcGquqBUogY2TR3cMwaoIzTj+yx4mzFbGKQ8NFYIlH8upOOdxVF1kXmCFxaF+TsPJS5HFOAd/DXF+ASt2lUGKA==;EndpointSuffix=core.windows.net";
            const string storageContainerName = "jag-test";
            
             
            var storageClient = new BlobContainerClient(
                storageConnectionString, storageContainerName);
            var eventProcessorClient = new EventProcessorClient(
                storageClient,
                consumerGroup,
                connectionString,
                eventHubName);
   
            eventProcessorClient.ProcessEventAsync += ProcessEventHandler;
            eventProcessorClient.ProcessErrorAsync += ProcessErrorHandler;
   
            await eventProcessorClient.StartProcessingAsync();
   
            await Task.Delay(TimeSpan.FromSeconds(30));
   
            await eventProcessorClient.StopProcessingAsync();
   
            
   
            Task ProcessEventHandler(ProcessEventArgs eventArgs)
            {
                var submission = JsonSerializer.Deserialize<RequestSubmisson>(Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()));
                
                var task =  _hubContext.Clients.All.ReceiveNotification($"Submission has been received {submission.SubmissionName}");
                task.Wait();
                
                var data = _taskService.PerformTasKAsync(new TodoItem
                {
                    Type1 = submission.Type1,
                    Type2 = submission.Type2
                });
                data.Wait();
                var task2 =  _hubContext.Clients.All.ReceiveNotification($"Submission has been processed {submission.SubmissionName} result {data.Result}");
                task2.Wait();
                return data;
            }
   
            Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
            {
             
                
                var task =  _hubContext.Clients.All.ReceiveNotification($"Failed to process event {eventArgs.Exception.Message}");
                task.Wait();
                
                // Return a completed task
                return task;
            }
            
           
        }
    }
}
/*
 *
   
   // ...
   
 
  
 *
 * 
 */