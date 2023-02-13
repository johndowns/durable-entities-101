using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using DurableEntities101.Entities;

namespace DurableEntities101
{
    public static class Function1
    {
        // Event Grid Functions

        [FunctionName("FileAddedToFolder")] // TODO make this into an Event Grid trigger
        public static async Task<IActionResult> Run( 
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation("Event Grid trigger received notification of file added to folder.");

            var folderName = "my-folder2";
            var dateTimeUtc = DateTime.UtcNow;

            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync(nameof(StartConnection), null, (folderName, dateTimeUtc));

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


        // Orchestrations

        [FunctionName("StartConnection")]
        public static async Task StartConnection(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var folderWriteEvent = context.GetInput<(string folderName, DateTime dateTimeUtc)>();
            
            var entityId = new EntityId(nameof(BlobFolderEntity), folderWriteEvent.folderName);
            var entityProxy = context.CreateEntityProxy<IBlobFolderEntity>(entityId);

            await entityProxy.FileUploaded(folderWriteEvent.dateTimeUtc);
        }
    }
}
