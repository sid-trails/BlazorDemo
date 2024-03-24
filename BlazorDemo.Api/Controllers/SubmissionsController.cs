using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using ServiceReference;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController: ControllerBase
{
    public ITaskService TaskService { get; }
    public SubmissionsController(ITaskService taskService)
    {
        TaskService = taskService;
    }
    
    [HttpPost]
    public async Task<IActionResult> SubmitProduct([FromBody] RequestSubmisson submission)
    {
        var producerClient = new EventHubProducerClient(
            "endpoint=sb://task-service.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=hlysiD2Hfkt9yZW0Y9WqHWI5VnrAF88Je+AEhCoOHGo=",
            "jag-test");
        using var eventBatch = await producerClient.CreateBatchAsync();

        eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes($"{JsonSerializer.Serialize(submission)}")));

        try
        {
            // Use the producer client to send the batch of events to the event hub
            await producerClient.SendAsync(eventBatch);
           
        }
        finally
        {
            await producerClient.DisposeAsync();
        }
        
        return Ok("Queued");
    }
    
    
}
public class RequestSubmisson
{
    public string SubmissionName { get; set; }
    public int  Type1 { get; set; }
    public int Type2 { get; set; }
    
}