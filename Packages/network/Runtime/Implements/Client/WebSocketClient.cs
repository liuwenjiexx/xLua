using System.Net;
using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.Enums;


namespace Yoozoo.Managers.V2.Core
{
    public class WebSocketClient: INetClient
    {
        public int PacketHeaderLength { get; set; }
        public bool AutoResendFailedPacket { get; set; }
        public AddressFamilyType addressFamily { get; set; }

        public void Connect(IPAddress ipAddress, int port, object userData)
        {
            throw new System.NotImplementedException();
        }

        public void Close()
        {
            throw new System.NotImplementedException();
        }

        public void Send(IPacket packet)
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public bool IsAvailable { get; }


        public event NetDelegates.NetClientDelegates.NetClientConnectedHandler OnNetClientConnected;
        public event NetDelegates.NetClientDelegates.NetClientErrorHandler OnNetClientError;
        public event NetDelegates.NetClientDelegates.NetClientClosedHandler OnNetClientClosed;
        public event NetDelegates.NetClientDelegates.NetClientReceivePacket OnNetClientReceivePacket;
        public NetDelegates.HelperDelegates.DeserializeHeaderHandler deserializeHeaderHandler { get; set; }
        public NetDelegates.HelperDelegates.DeserializePacketHandler deserializePacketHandler { get; set; }
        public NetDelegates.HelperDelegates.SerializeHandler serializePacketHandler { get; set; }
        public NetDelegates.HelperDelegates.SerializeHandler serializeContentHandler { get; set; }
    }
}