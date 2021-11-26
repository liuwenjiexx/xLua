using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.Core;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public interface IReconnectHelper
    {
        /// <summary>
        /// 当心跳未能正常收到时
        /// </summary>
        void OnLoseHeartBeat(NetChannel channel,INetClient client,int loseCount);
        
        
        /// <summary>
        /// 当连接被关闭时
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="client"></param>
        void OnLoseConnections(NetChannel channel,INetClient client);
        
        
        
        /// <summary>
        /// 当连接建立时
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="client"></param>
        void OnConnectionSuccess(NetChannel channel,INetClient client, object customData);
        
        
        
        void Update(NetChannel channel,INetClient client);
        
        
        void Reset(NetChannel channel,INetClient client);


        event NetDelegates.HelperDelegates.ReconnectedHandler OnReconnected;
    }
}