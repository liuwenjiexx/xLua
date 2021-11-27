using System;
using System.Collections.Generic;
using System.IO;
using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.DataStruct;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public class CustomPacketHelper : IPacketSerializedHelper, IPacketBodyHelper
    {
        private readonly MemoryStream m_SerializeCachedStream = new MemoryStream(1024 * 8);

        public static bool useBigEndian = false;

        private static bool? reverseEndian;
        public static bool ReverseEndian
        {
            get
            {
                if (!reverseEndian.HasValue)
                {
                    reverseEndian = false;
                    if (useBigEndian)
                    {
                        if (BitConverter.IsLittleEndian)
                        {
                            reverseEndian = true;
                        }
                    }
                    else
                    {
                        if (!BitConverter.IsLittleEndian)
                        {
                            reverseEndian = true;
                        }
                    }
                }
                return reverseEndian.Value;
            }
        }

        byte[] GetBytes(int n)
        {
            byte[] bytes = BitConverter.GetBytes(n);
            if (ReverseEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
        static int ToInt32(byte[] buffer, int offset = 0)
        {
            if (ReverseEndian)
            {
                Array.Reverse(buffer, offset, 4);
            }
            return BitConverter.ToInt32(buffer, offset);
        }
        public bool SerializePacketHandler(IPacket packet, Stream destination)
        {
            m_SerializeCachedStream.SetLength(m_SerializeCachedStream.Capacity); // 此行防止 Array.Copy 的数据无法写入
            m_SerializeCachedStream.Position = 0L;

            //total length
            int headLength = 4;
            m_SerializeCachedStream.Position = headLength;

            m_SerializeCachedStream.Write(GetBytes(packet.Id), 0, 4);

            if (packet.rawData != null)
            {
                var bytes = packet.rawData;
                m_SerializeCachedStream.Write(bytes, 0, bytes.Length);
            }

            int bodyLength = (int)m_SerializeCachedStream.Position - headLength;
            m_SerializeCachedStream.Position = 0;
            m_SerializeCachedStream.Write(GetBytes(bodyLength), 0, 4);

            m_SerializeCachedStream.Position = 0;
            m_SerializeCachedStream.SetLength(headLength + bodyLength);
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

        public IPacket DeserializePacketCore(IPacketHeader header, Stream source, out object customErrorData)
        {
            customErrorData = null;
            IPacket packet = new BasicPacket();
            byte[] tmp = new byte[8];
            source.Read(tmp, 0, 4);
            packet.Id = ToInt32(tmp);

            int dataOffset = 4;
            var bodybytes = new byte[header.PacketLength - dataOffset];
            source.Read(bodybytes, 0, bodybytes.Length);
            packet.rawData = bodybytes;
            return packet;
        }

        public IPacket DeserializePacketContent(IPacket packet, out object customErrorData)
        {
            customErrorData = null;

            packet.message = packet.rawData;
            return packet;
        }
    }
}