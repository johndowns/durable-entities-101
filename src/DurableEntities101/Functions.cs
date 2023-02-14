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
    public static class Functions
    {
        // Event Grid Functions

        [FunctionName("FileAddedToFolder")] // TODO make this into an Event Grid trigger
        public static async Task<IActionResult> Run( 
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log)
        {
            log.LogInformation("Event Grid trigger received notification of file added to folder.");

            var folderName = "my-folder4";
            var dateTimeUtc = DateTime.UtcNow;

            // Signal the entity to tell it that a new blob has been added to the folder.
            var entityId = new EntityId(nameof(BlobFolderEntity), folderName);
            await entityClient.SignalEntityAsync(entityId, nameof(BlobFolderEntity.FileUploaded), dateTimeUtc);

            return new OkResult();
        }
    }
}
