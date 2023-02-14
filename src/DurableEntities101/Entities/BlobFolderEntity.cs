using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using DurableEntities101.Enums;
using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;

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
        public static Task Run(
            [EntityTrigger] IDurableEntityContext ctx,
            [ServiceBus("processblobfolder", Connection = "MyConnection")] Azure.Messaging.ServiceBus.ServiceBusSender serviceBusSender,
            ILogger log)
        {
            return ctx.DispatchAsync<BlobFolderEntity>(ctx, serviceBusSender, log);
        }
        #endregion

        #region Constructor
        public BlobFolderEntity(IDurableEntityContext context, ServiceBusSender serviceBusSender, ILogger log)
        {
            _context = context;
            _serviceBusSender = serviceBusSender;
            _log = log;
        }
        #endregion

        #region Fields
        private readonly IDurableEntityContext _context;

        private readonly ServiceBusSender _serviceBusSender;

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

        private const int ExpectedFileCount = 3;
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
            return _serviceBusSender.SendMessageAsync(new ServiceBusMessage(_context.EntityKey));
        }
    }
}
