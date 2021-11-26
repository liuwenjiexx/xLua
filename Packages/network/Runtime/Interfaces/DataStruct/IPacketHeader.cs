//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace Yoozoo.Managers.NetworkV2.DataStruct
{
    public interface IPacketHeader
    {
        /// <summary>
        /// 包头的长度
        /// </summary>
        int HeaderByteLength { get; set; }

        /// <summary>
        /// 获取网络消息包长度。不包括包头长度
        /// </summary>
        int PacketLength { get; set; }
    }
}