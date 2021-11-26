//-----------------------------------------------------------------------
// Created By 甘道夫
// contact E-mail: wwei@yoozoo.com
// Date: 2020-12-31
// 默认序列化辅助类
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.DataStruct;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public class DefaultPacketSerializerHelper : IPacketSerializedHelper
    {
        // private ProtobufSerializer m_Serializer = new ProtobufSerializer();
        private readonly MemoryStream m_SerializeCachedStream = new MemoryStream(1024 * 8);
        public bool SerializePacketHandler(IPacket packet, Stream destination)
        {
            m_SerializeCachedStream.SetLength(m_SerializeCachedStream.Capacity); // 此行防止 Array.Copy 的数据无法写入
            m_SerializeCachedStream.Position = 0L;
            
            //TEMP： 服务器使用的是字符串，临时处理，项目组不要抄袭
            var dic=new Dictionary<int,string>()
            {
                {1,"ProtoCommon.GmCommand"},
                {10001,"ProtoCommon.LoginRequest"},
                {10002,"ProtoCommon.CreatePlayerRequest"},
                {10003,"ProtoCommon.LoadingDataRequest"},
                {10004,"ProtoCommon.ActorLevelUpRequest"}
            };
            var msgStrBytes = System.Text.Encoding.UTF8.GetBytes(dic[packet.Id]);
            var msgStrLengthBytes =new byte[1]{(byte)msgStrBytes.Length};// BitConverter.GetBytes(msgStrBytes.Length);
            
            m_SerializeCachedStream.Write(msgStrLengthBytes,0,1);
            m_SerializeCachedStream.Write(msgStrBytes,0,msgStrBytes.Length);
            //-------------------------------------------------
            
            if (packet.Id>10000)
            {
                if (packet.message!=null)
                {
                    var bytes = packet.message as byte[];
                    m_SerializeCachedStream.Write(bytes,0,bytes.Length);
                }
            }
            // else
            // {
            //     m_Serializer.Serialize(m_SerializeCachedStream,packet.message);
            // }
            int totalLength = (int)m_SerializeCachedStream.Position;
            var totalLengthBytes = BitConverter.GetBytes(totalLength);
            var bodyBytes = new byte[(int)m_SerializeCachedStream.Position];
            m_SerializeCachedStream.Seek(0, SeekOrigin.Begin);
            m_SerializeCachedStream.Read(bodyBytes, 0, bodyBytes.Length);
            m_SerializeCachedStream.Position = 0;
            m_SerializeCachedStream.Write(totalLengthBytes,0,4);
            m_SerializeCachedStream.Write(bodyBytes,0,bodyBytes.Length);
            m_SerializeCachedStream.SetLength(totalLength+4);
            m_SerializeCachedStream.WriteTo(destination);

            return true;
        }

        public bool SerializeContentHandler(IPacket packet, Stream destination)
        {
            return true;
        }

        public void ResetOnConnected(NetChannel channel, INetClient client, object data)
        {
            
        }
    }
}