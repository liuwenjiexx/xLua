//-----------------------------------------------------------------------
// Created By 甘道夫
// contact E-mail: wwei@yoozoo.com
// Date: 2020-12-31
// 默认包体反序列化辅助类
//-----------------------------------------------------------------------
using System.IO;
using Yoozoo.Managers.NetworkV2.DataStruct;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public class DefaultPacketBodyHelper : IPacketBodyHelper
    {
        public virtual IPacket DeserializePacketCore(IPacketHeader header, Stream source, out object customErrorData)
        {
            customErrorData = null;
            var bodybytes=new byte[header.PacketLength];
            source.Read(bodybytes, 0, bodybytes.Length);
            return new BasicPacket()
            {
                Id = 0,
                // message = bodybytes,
                rawData = bodybytes
            };
        }

        public virtual IPacket DeserializePacketContent(IPacket packet, out object customErrorData)
        {
            customErrorData = null;
            //在这里实现包体解析，比如用protobuf解析，解析后方会message下
            packet.message = packet.rawData;
            return packet;
        }
    }
}