//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace Yoozoo.Managers.NetworkV2.DataStruct
{
    /// <summary>
    /// 网络消息包基类。
    /// </summary>
    public abstract class IPacket
    {
        public IPacketHeader header;
        public int Id;
        public object message;
        public byte[] rawData = null;
    }
}
