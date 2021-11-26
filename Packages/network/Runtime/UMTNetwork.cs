//-----------------------------------------------------------------------
// Created By 甘道夫
// contact E-mail: wwei@yoozoo.com
// Date: 2020-12-31
// 网络库接口
//-----------------------------------------------------------------------

using Yoozoo.Managers.NetworkV2.Core;

namespace Yoozoo.Managers.NetworkV2
{
    public class UMTNetwork
    {
        
        /// <summary>
        /// 是否拥有一条网络信道
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public static bool HasNetworkChannel(string channelName)
        {
            return NetworkEntry.HasNetworkChannel(channelName);
        }
        
        
        /// <summary>
        /// 获取一个网络信道
        /// </summary>
        /// <param name="channelName">通道名</param>
        /// <returns></returns>
        public static NetChannel GetChannel(string channelName)
        {
            return NetworkEntry.GetChannel(channelName);
        }

        /// <summary>
        /// 注册一个网络信道
        /// </summary>
        /// <param name="channel">通道对象</param>
        public static void RegisterChannel(NetChannel channel)
        {
            NetworkEntry.RegisterChannel(channel);
        }

        
        /// <summary>
        /// 通过通道发送一组bytes
        /// </summary>
        /// <param name="channelName">通道名</param>
        /// <param name="msgId"></param>
        /// <param name="bytes"></param>
        public static void SendBytes(string channelName, int msgId, byte[] bytes)
        {
            NetworkEntry.GetChannel(channelName)?.SendBytes(msgId, bytes);
        }

        
        /// <summary>
        /// 通过通道发送一组数据对象(使用序列化helper来帮助)
        /// </summary>
        /// <param name="channelName">通道名</param>
        /// <param name="msgId"></param>
        /// <param name="data"></param>
        public static void Send(string channelName, int msgId, object data)
        {
            NetworkEntry.GetChannel(channelName)?.Send(msgId, data);
        }



        /// <summary>
        /// 开启运行一个网络通道
        /// </summary>
        /// <param name="channelName">通道名</param>
        /// <param name="customData"></param>
        public static void StartUp(string channelName, object customData = null)
        {
            NetworkEntry.GetChannel(channelName)?.StartUp(customData);
        }

        
        /// <summary>
        /// 暂时关闭一个网络通道
        /// </summary>
        /// <param name="channelName">通道名</param>
        public static void ShutDown(string channelName)
        {
            NetworkEntry.GetChannel(channelName)?.ShutDown();
        }
        
        

    }
}