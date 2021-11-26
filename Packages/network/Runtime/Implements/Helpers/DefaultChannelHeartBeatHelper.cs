//-----------------------------------------------------------------------
// Created By 甘道夫
// contact E-mail: wwei@yoozoo.com
// Date: 2020-12-31
// 默认心跳辅助类
//-----------------------------------------------------------------------

using UnityEngine;
using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.States;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public class DefaultChannelHeartBeatHelper : IHeartBeatHelper
    {
        protected readonly NetClient.HeartBeatState m_HeartBeatState = new NetClient.HeartBeatState();

        public event NetDelegates.HelperDelegates.MissHeartBeatHandler onMissHeartBeat;
        public float HeartBeatInterval { get; set; } = 5f;

        public virtual bool ResetByAnyReceive
        {
            get { return true; }
        }

        public void Update(NetChannel channel,INetClient client,float deltaTm)
        {
            if (channel == null || client == null) return;
            if (HeartBeatInterval > 0f)
            {
                bool sendHeartBeat = false;
                int missHeartBeatCount = 0;
                
                
                lock (m_HeartBeatState)
                {

                    if (!client.IsAvailable)
                    {
                        return;
                    }

                    m_HeartBeatState.HeartBeatElapseSeconds += deltaTm;
                    if (m_HeartBeatState.HeartBeatElapseSeconds >= HeartBeatInterval)
                    {
                        sendHeartBeat = true;
                        missHeartBeatCount = m_HeartBeatState.MissHeartBeatCount;
                        m_HeartBeatState.HeartBeatElapseSeconds = 0f;
                        m_HeartBeatState.MissHeartBeatCount++;
                    }
                }

                if (sendHeartBeat )
                {
                    if (SendHeartBeat(channel, client))
                    {
                        m_HeartBeatState.Reset(true);
                        missHeartBeatCount = m_HeartBeatState.MissHeartBeatCount;
                        return;
                    }

                    if (missHeartBeatCount > 0)
                    {
                        OnBeforeMissHeartBeatTrig(channel,client,missHeartBeatCount);
                    }
                }
            }
        }

        public void OnBeforeMissHeartBeatTrig(NetChannel channel,INetClient client,int missHeartBeatCount)
        {
            if (missHeartBeatCount >= 10)
            {
                //这里改为使用重连逻辑
                Debug.Log("miss Heart beat too much, auto shutdown the channel");
                channel.ShutDownInternal(false);
            }
            onMissHeartBeat?.Invoke(missHeartBeatCount);
        }

        public virtual void Init(NetChannel channel){
        
        }

        /// <summary>
        /// 发送心跳消息包。
        /// </summary>
        /// <returns>是否发送心跳消息包成功。</returns>
        public virtual bool SendHeartBeat(NetChannel channel,INetClient client)
        {
            //todo 发送心跳消息
            Debug.Log("模拟发送了一个心跳包（实际没法）");
            return true;
        }

        public void ResetHeartBeat(bool isByReceiveAnyPacket)
        {
            if (ResetByAnyReceive)
            {
                //receive可以认为有心跳 5s没有收到消息才发送
                lock (m_HeartBeatState)
                {
                    m_HeartBeatState.Reset(true);
                }
            }
            else
            { //心跳测试 不依赖receive 固定5s一个
                lock (m_HeartBeatState)
                { 
                    if(!isByReceiveAnyPacket) 
                        m_HeartBeatState.Reset(true);
                }
            }
        }
    }
}