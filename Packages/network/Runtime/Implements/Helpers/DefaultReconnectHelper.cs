//-----------------------------------------------------------------------
// Created By 甘道夫
// contact E-mail: wwei@yoozoo.com
// Date: 2020-12-31
// 默认重连辅助类
//-----------------------------------------------------------------------

using UnityEngine;
using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.Helpers;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public class DefaultReconnectHelper : IReconnectHelper
    {
        private class ReconnectionInfo
        {
            public ReconnectionInfo(int connectionTimes)
            {
                this.ConnectionTimes = connectionTimes;
            }

            public int ConnectionTimes = 0;
        }

        protected virtual int MaxReconnectionTryTimes
        {
            get { return 5; }
        }
        
        protected virtual float MinReconnectionInterval
        {
            get { return 5; }
        }
        
        private int m_TryReconnectionCnt = 0;
        private float m_lastReconnectionTime = -1;

        private bool m_WaitReconnection = false;
        public void OnLoseHeartBeat(NetChannel channel, INetClient client, int loseCount)
        {
            
                if (!m_WaitReconnection && CheckReconnectAbleOnLoseHeartBeat(channel,client,loseCount))
                {
                    m_WaitReconnection = true;
                    
                }
                else if (!m_WaitReconnection)
                {
                    channel.ShutDown();
                }
        }

        public void OnLoseConnections(NetChannel channel, INetClient client)
        {
            if (!m_WaitReconnection && CheckReconnectAbleOnLoseConnection(channel, client))
            {
                m_WaitReconnection = true;
            }
            else if (!m_WaitReconnection)
            {
                channel.ShutDown();
            }
        }

        public void OnConnectionSuccess(NetChannel channel, INetClient client,object customData)
        {
            var reconnectionData = customData as ReconnectionInfo;
            Reset(channel, client);
            m_lastReconnectionTime = Time.realtimeSinceStartup;
            if (reconnectionData != null)
            {
                OnReconnected?.Invoke();
                SendReconnectProtocals(channel, client);
            }
        }


        public void TryReconnection(NetChannel channel, INetClient client)
        {

            if (m_TryReconnectionCnt >= MaxReconnectionTryTimes)
            {
                channel.ShutDownInternal(false);
                return;
            }

            if (m_lastReconnectionTime + MinReconnectionInterval > Time.realtimeSinceStartup)
            {
                m_WaitReconnection = true;
                return;
            }
            
            m_WaitReconnection = false;
            m_TryReconnectionCnt++;
            m_lastReconnectionTime = Time.realtimeSinceStartup;
            channel.StartUpInternal(new ReconnectionInfo(m_TryReconnectionCnt));
            Debug.Log($"尝试重新连接{m_TryReconnectionCnt}");
        }

        public void Update(NetChannel channel, INetClient client)
        {
            if (m_WaitReconnection)
            {
                TryReconnection(channel,client);
            }
        }

        public void Reset(NetChannel channel, INetClient client)
        {
            m_TryReconnectionCnt = 0;
            m_WaitReconnection = false;
        }

        public event NetDelegates.HelperDelegates.ReconnectedHandler OnReconnected;


        
        protected virtual void SendReconnectProtocals(NetChannel channel, INetClient client)
        {
            Debug.Log("发送重连所需协议");
        }
        
        
        protected virtual bool CheckReconnectAbleOnLoseHeartBeat(NetChannel channel, INetClient client, int loseCount)
        {
            if (loseCount >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool CheckReconnectAbleOnLoseConnection(NetChannel channel, INetClient client)
        {
            return true;
        }

    }
}