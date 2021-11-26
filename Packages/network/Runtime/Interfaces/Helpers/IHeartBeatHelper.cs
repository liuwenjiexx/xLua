using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.Core;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public interface IHeartBeatHelper
    {
        void Init(NetChannel channel);
        
        event NetDelegates.HelperDelegates.MissHeartBeatHandler onMissHeartBeat;
        float HeartBeatInterval { get; set; }
        void Update(NetChannel channel,INetClient client,float deltaTm);
        
        /// <summary>
        /// 发送心跳消息包。
        /// </summary>
        /// <returns>是否发送心跳消息包成功。</returns>
        bool SendHeartBeat(NetChannel channel,INetClient client);
        
        void ResetHeartBeat(bool isByReceiveAnyPacket);

        void OnBeforeMissHeartBeatTrig(NetChannel channel, INetClient client, int missHeartBeatCount);
    }
}