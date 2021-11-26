//-----------------------------------------------------------------------
// Created By 甘道夫
// contact E-mail: wwei@yoozoo.com
// Date: 2020-12-31
// 网络底层封装适配层接口
//-----------------------------------------------------------------------

using System.Net;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.Enums;

namespace Yoozoo.Managers.NetworkV2.Client
{
    public interface INetClient
    {
        int PacketHeaderLength { get; set; }

        bool AutoResendFailedPacket { get; set; }

        AddressFamilyType addressFamily { set; get; }
        void Connect(IPAddress ipAddress, int port, object userData);
        void Close();

        void Send(IPacket packet);
        
        void Update();

        bool IsAvailable { get; }

        event NetDelegates.NetClientDelegates.NetClientConnectedHandler OnNetClientConnected;
        
        event NetDelegates.NetClientDelegates.NetClientErrorHandler OnNetClientError;
        
        event NetDelegates.NetClientDelegates.NetClientClosedHandler OnNetClientClosed;
        
        event NetDelegates.NetClientDelegates.NetClientReceivePacket OnNetClientReceivePacket;

        
        NetDelegates.HelperDelegates.DeserializeHeaderHandler deserializeHeaderHandler { get; set; }
        NetDelegates.HelperDelegates.DeserializePacketHandler deserializePacketHandler { get; set; }
        NetDelegates.HelperDelegates.SerializeHandler serializePacketHandler { get; set; }
        NetDelegates.HelperDelegates.SerializeHandler serializeContentHandler { get; set; }
        
    }
}