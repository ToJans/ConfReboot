using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Azure.Common;
using ConfReboot;
using ConfReboot.Infrastructure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;

namespace CommandProcessor
{
    public class WorkerRole : RoleEntryPoint
    {
        Bus DomainBus;
        CloudQueue queue;
        AzureBlobES store;

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("$projectname$ entry point called", "Information");

            while (true)
            {
                foreach (var azuremsg in queue.GetMessages(100))
                {
                    var msg = azuremsg.AsBytes.ToMessage();
                    DomainBus.HandleUntilAllConsumed(msg, store.EmitMessage, store.FindMsgs);
                }

                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            DomainBus = new Bus();
            DomainBus.RegisterType<Conference>();
            DomainBus.RegisterType<ConferenceOrderSaga>();
            DomainBus.RegisterType<Order>();

            var csa = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            queue = ResolveQueue(csa, "commands");
            store = new AzureBlobES(csa);

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        private CloudQueue ResolveQueue(CloudStorageAccount storageAccount, string queuename)
        {
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(queuename);
            queue.CreateIfNotExist();
            return queue;
        }

        private class AzureBlobES
        {
            public AzureBlobES(CloudStorageAccount csa)
            {
                var cbc = csa.CreateCloudBlobClient();
                EventBlobContainer = cbc.GetContainerReference("events");
                EventBlobContainer.CreateIfNotExist();
            }

            string keyname = null;
            private CloudBlobContainer EventBlobContainer;

            public IEnumerable<Message> FindMsgs(IEnumerable<KeyValuePair<string, object>> Keys)
            {
                keyname = new Message("Events", Keys).ToFriendlyString();
                var bbr = EventBlobContainer.GetBlobReference(keyname);
                using (var str = bbr.OpenRead())
                {
                    var msg = ProtoBuf.Serializer.DeserializeWithLengthPrefix<Message>(str, ProtoBuf.PrefixStyle.Fixed32BigEndian);
                    yield return msg;
                }
                yield break;
            }

            public void EmitMessage(Message msg)
            {
                var bbr = EventBlobContainer.GetBlobReference(keyname);
                using (var str = bbr.OpenWrite())
                {
                    str.Seek(0, System.IO.SeekOrigin.End);
                    ProtoBuf.Serializer.SerializeWithLengthPrefix<Message>(str, msg, ProtoBuf.PrefixStyle.Fixed32BigEndian);
                }
            }
        }
    }
}