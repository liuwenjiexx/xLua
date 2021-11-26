﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------


namespace Yoozoo.Managers.NetworkV2.DataStruct
{
    /// <summary>
    /// 网络消息包头接口。
    /// </summary>
    public struct BasicPacketHeader : IPacketHeader
    {
        
        /// <summary>
        /// 包头的长度
        /// </summary>
        public int HeaderByteLength { get; set; }

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        public int PacketLength { get; set; }
    }
}
