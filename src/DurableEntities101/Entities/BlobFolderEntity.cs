using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using DurableEntities101.Enums;
using Microsoft.Extensions.Logging;

namespace DurableEntities101.Entities
{
    public interface IBlobFolderEntity
    {
        Task FileUploaded(DateTime timeReceived);
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class BlobFolderEntity : IBlobFolderEntity
    {
        #region Entity Function
        [FunctionName(nameof(BlobFolderEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext ctx, ILogger log)
        {
            return ctx.DispatchAsync<BlobFolderEntity>(ctx, log);
        }
        #endregion

        #region Constructor
        public BlobFolderEntity(IDurableEntityContext context, ILogger log)
        {
            _context = context;
            _log = log;
        }
        #endregion

        #region Fields
        private readonly IDurableEntityContext _context;

        private readonly ILogger _log;
        #endregion

        #region Properties
        [JsonProperty("fileCount")]
        public int FileCount { get; set; }

        [JsonProperty("lastReceivedFileTimeStamp")]
        public DateTime LastReceivedFileTimeStampUtc { get; set; }

        [JsonProperty("state")]
        public BlobFolderState State { get; set; }
        #endregion

        #region Constants
        private readonly TimeSpan PollingFrequency = TimeSpan.FromSeconds(10);

        private readonly TimeSpan UploadTimeout = TimeSpan.FromMinutes(5);

        private const int ExpectedFileCount = 15;
        #endregion

        public async Task FileUploaded(DateTime timeReceivedUtc)
        {
            FileCount++;
            LastReceivedFileTimeStampUtc = timeReceivedUtc;

            await CheckLastUpdateTime();
        }

        public async Task CheckLastUpdateTime()
        {
            if (State == BlobFolderState.Done)
            {
                return;
            }
            
            if (FileCount >= ExpectedFileCount || LastReceivedFileTimeStampUtc <= DateTime.UtcNow.Subtract(UploadTimeout))
            {
                await SendServiceBusMessage();
                State = BlobFolderState.Done;
                return;
            }

            // Automatically signal the entity again in a few seconds.
            _context.SignalEntity(_context.EntityId, DateTime.UtcNow.Add(PollingFrequency), nameof(CheckLastUpdateTime));
        }

        public Task SendServiceBusMessage()
        {
            _log.LogInformation("This is where I'd send a message to Service Bus :)");
            return Task.CompletedTask;
        }
    }
}
