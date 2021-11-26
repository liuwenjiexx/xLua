using System;
using System.Collections.Generic;
using System.IO;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.Helpers;

namespace Yoozoo.Framework.Network.Helpers
{
    public class HDServerPackDeserializerHelper : IPacketHeaderHelper, IPacketBodyHelper
    {
        public int PackageHeaderLength => 4;

        private Dictionary<string, ushort> m_ProtoIdMap = new Dictionary<string, ushort>()
        {
            {"ProtoCommon.LoginReply", 10001},
            {"ProtoCommon.CreatePlayerReply", 10002},
            {"ProtoCommon.Player", 10003},
            {"ProtoCommon.FormationTeam", 10004},
            {"ProtoCommon.ErrorInfo", 10005},
            {"ProtoCommon.SyncrolizeActorDataInt32", 10006},
            {"ProtoCommon.DataActorAttribute", 10007}
        };

        // private Dictionary<int, Type> m_IdProtoMaps = new Dictionary<int, Type>()
        // {
        //     {1, typeof(ProtoAspenDungeon.GmCommand)}
        // };


        public IPacketHeader DeserializeHeader(Stream source, out object customErrorData)
        {
            int headerBytesLenth = 0;
            customErrorData = null;
            int totalLength = ReadInt(source, ref headerBytesLenth);
            return new BasicPacketHeader()
            {
                PacketLength = totalLength - 4,
                HeaderByteLength = PackageHeaderLength
            };
        }


        public IPacket DeserializePacketCore(IPacketHeader header, Stream source, out object customErrorData)
        {
            customErrorData = null;

            int readedLength = 0;

            int msgId = ReadInt(source, ref readedLength);
            UnityEngine.Debug.Log("Reveive:  " + msgId);


            int bodyLength = header.PacketLength - 4;

            if (bodyLength >= 0)
            {
                byte[] bodybytes = new byte[bodyLength];
                source.Read(bodybytes, 0, bodybytes.Length);

                return new BasicPacket()
                {
                    Id = msgId,
                    rawData = bodybytes
                };
            }
            else
            {
                return new BasicPacket()
                {
                    Id = msgId,
                    message = null
                };
            }
        }

        //
        // private ProtobufSerializer m_Serializer = new ProtobufSerializer();
        // private readonly MemoryStream m_DeserializeCachedStream = new MemoryStream(1024 * 8);

        public IPacket DeserializePacketContent(IPacket packet, out object customErrorData)
        {
            customErrorData = null;
            //在这里实现包体解析，比如用protobuf解析，解析后方会message下
            // m_DeserializeCachedStream.SetLength(m_DeserializeCachedStream.Capacity); // 此行防止 Array.Copy 的数据无法写入
            // m_DeserializeCachedStream.Position = 0L;
            // m_DeserializeCachedStream.Write(packet.rawData, 0, packet.rawData.Length);
            // packet.message = m_Serializer.Deserialize(m_DeserializeCachedStream, null, m_IdProtoMaps[packet.Id]);
            packet.message = packet.rawData;
            return packet;
        }


        private int ReadInt(Stream source, ref int headerBytesLength)
        {
            byte[] bytes = new byte[4];
            source.Read(bytes, 0, 4);
            int value = BitConverter.ToInt32(bytes, 0);
            headerBytesLength += 4;
            return value;
        }
    }
}