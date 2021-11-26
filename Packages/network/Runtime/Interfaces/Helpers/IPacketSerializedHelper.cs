using System.IO;
using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.DataStruct;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public interface IPacketSerializedHelper
    {
        bool SerializePacketHandler(IPacket packet,Stream destination);
        bool SerializeContentHandler(IPacket packet,Stream destination);
        void ResetOnConnected(NetChannel channel,INetClient client, object data);
    }
}