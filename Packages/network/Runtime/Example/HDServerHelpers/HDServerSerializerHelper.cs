using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.Helpers;

namespace Yoozoo.Framework.Network.Helpers
{
    public class HDServerSerializerHelper : IPacketSerializedHelper
    {
        public bool SerializePacketHandler(IPacket packet, Stream destination)
        {
            m_SerializeCachedStream.SetLength(m_SerializeCachedStream.Capacity); // 此行防止 Array.Copy 的数据无法写入
            m_SerializeCachedStream.Position = 0L;
            
            var msgIDBytes = BitConverter.GetBytes(packet.Id);
            
            m_SerializeCachedStream.Write(msgIDBytes, 0, msgIDBytes.Length);
            //-------------------------------------------------
            m_SerializeCachedStream.Write(packet.rawData, 0, packet.rawData.Length);


            int totalLength = (int) m_SerializeCachedStream.Position;
            var totalLengthBytes = BitConverter.GetBytes(totalLength+4);
            var bodyBytes = new byte[(int) m_SerializeCachedStream.Position];

            m_SerializeCachedStream.Seek(0, SeekOrigin.Begin);
            m_SerializeCachedStream.Read(bodyBytes, 0, bodyBytes.Length);


            m_SerializeCachedStream.Position = 0L;
            m_SerializeCachedStream.Write(totalLengthBytes, 0, 4);
            m_SerializeCachedStream.Write(bodyBytes, 0, bodyBytes.Length);
            m_SerializeCachedStream.SetLength(totalLength + 4);
            m_SerializeCachedStream.WriteTo(destination);

            return true;
        }


        // private ProtobufSerializer m_Serializer = new ProtobufSerializer();
        private readonly MemoryStream m_SerializeCachedStream = new MemoryStream(1024 * 8);

        public bool SerializeContentHandler(IPacket packet, Stream destination)
        {
            m_SerializeCachedStream.SetLength(m_SerializeCachedStream.Capacity); // 此行防止 Array.Copy 的数据无法写入
            m_SerializeCachedStream.Position = 0L;
            // m_Serializer.Serialize(m_SerializeCachedStream, packet.message);
            var bytes = new byte[(int) m_SerializeCachedStream.Position];
            packet.rawData = bytes;
            packet.message = null;

            return true;
        }

        public void ResetOnConnected(NetChannel channel, INetClient client, object data)
        {
            
        }
    }
}