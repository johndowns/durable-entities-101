using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using DurableEntities101.Entities;

namespace DurableEntities101
{
    public static class Functions
    {
        const string BlobSubjectFolderNameRegex = "\\/blobServices\\/default\\/containers\\/.+\\/blobs\\/(.+)\\/(.+)";

        [FunctionName("FileAddedToFolder")]
        public static async Task FileAddedToFolder(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            [DurableClient] IDurableEntityClient entityClient,
            ILogger log)
        {
            log.LogInformation("Event Grid trigger received notification of file added to folder.");

            // Get the name of the blob folder from the Event Grid message.
            var match = Regex.Match(eventGridEvent.Subject, BlobSubjectFolderNameRegex);
            var folderName = match.Groups[1].Value.Replace("/", "_");
            
            // Signal the entity to tell it that a new blob has been added to the folder.
            var entityId = new EntityId(nameof(BlobFolderEntity), folderName);
            await entityClient.SignalEntityAsync(entityId, nameof(BlobFolderEntity.FileUploaded), eventGridEvent.EventTime.DateTime);
        }
    }
}
