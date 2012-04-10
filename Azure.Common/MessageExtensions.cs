using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConfReboot.Infrastructure;
using ProtoBuf;

namespace Azure.Common
{
    [ProtoContract]
    public class ProtoMessage
    {
        [ProtoMember(0)]
        public string MethodName { get; set; }

        [ProtoMember(1)]
        public KeyValuePair<string, object>[] Parameters { get; set; }
    }

    public static class MessageExtensions
    {
        public static Message ToMessage(this byte[] input)
        {
            using (var ms = new MemoryStream(input))
            {
                var protomsg = Serializer.Deserialize<ProtoMessage>(ms);
                return new Message(protomsg.MethodName, protomsg.Parameters);
            }
        }

        public static byte[] ToByteArray(this Message input)
        {
            using (var ms = new MemoryStream())
            {
                var protomsg = new ProtoMessage { MethodName = input.MethodName, Parameters = input.Parameters.ToArray() };
                Serializer.Serialize(ms, protomsg);
                var bytes = ms.ToArray();
                return bytes;
            }
        }
    }
}