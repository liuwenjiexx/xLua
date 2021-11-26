using System.IO;
using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.Enums;

namespace Yoozoo.Managers.NetworkV2.Core
{
    public class NetDelegates
    {
        /// <summary>
        /// channel的事件的委托
        /// </summary>
        public class ChannelDelegates
        {
            public delegate void NetworkChannelConnectedHandler(NetChannel channel, object data);

            public delegate void NetworkChannelReconnectedHandler(NetChannel channel);

            public delegate void NetworkChannelClosedHandler(NetChannel channel);
            
            
            public delegate void NetworkChannelStartUpHandler(NetChannel channel);
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="channel"></param>
            /// <param name="shutDownByUser">是否是被用户主动关闭的，如重连失败等抛出为false</param>
            public delegate void NetworkChannelShutDownHandler(NetChannel channel,bool shutDownByUser);
           

            public delegate void NetworkChannelErrorHandler(NetChannel channel, NetworkErrorCode errorType, int errorCode, string errorMsg);
            
            public delegate void NetworkChannelReceivePacketHandler(NetChannel channel, IPacket packet);
        }

        /// <summary>
        /// NetClient的事件委托
        /// </summary>
        public class NetClientDelegates
        {
            public delegate void NetClientConnectedHandler(INetClient client, object data);
            
            public delegate void NetClientClosedHandler(INetClient client);
            
            public delegate void NetClientErrorHandler(INetClient client, NetworkErrorCode errorType, int errorCode, string errorMsg);
            
            public delegate void NetClientReceivePacket(INetClient client,IPacket packet);
           
        }
        
        
        public class HelperDelegates
        {
            public delegate IPacketHeader DeserializeHeaderHandler(Stream source,out object customErrorData);
            
            public delegate IPacket DeserializePacketHandler(IPacketHeader header,Stream source,out object customErrorData);
            
            public delegate bool SerializeHandler(IPacket data,Stream source);

            public delegate void MissHeartBeatHandler(int count);

            public delegate void ReconnectedHandler();
        }
    }
}