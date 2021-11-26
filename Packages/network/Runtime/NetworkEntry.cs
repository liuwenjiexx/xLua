//-----------------------------------------------------------------------
// Created By 甘道夫
// contact E-mail: wwei@yoozoo.com
// Date: 2020-12-31
// 通道管理器
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Yoozoo.Managers.NetworkV2.Core;

namespace Yoozoo.Managers.NetworkV2
{
    public static class NetworkEntry
    {
        private static Dictionary<string,NetChannel> m_channels = new Dictionary<string,NetChannel>();
        
        public static NetChannel GetChannel(string channelName)
        {
            if (m_channels.ContainsKey(channelName))
            {
                return m_channels[channelName];
            }
            else
            {
                return null;
            }
        }
        
        public static void RegisterChannel(NetChannel channel)
        {
            if (m_channels.ContainsKey(channel.Name))
            {
                m_channels[channel.Name].ShutDown(true);
            }
            m_channels[channel.Name] = channel;
        }


        public static bool HasNetworkChannel(string name)
        {
            return m_channels.ContainsKey(name) && m_channels[name]!=null;
        }
    }
}