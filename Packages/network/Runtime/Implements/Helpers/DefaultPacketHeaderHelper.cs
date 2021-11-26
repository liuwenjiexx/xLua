//-----------------------------------------------------------------------
// Created By 甘道夫
// contact E-mail: wwei@yoozoo.com
// Date: 2020-12-31
// 默认包头反序列化辅助类
//-----------------------------------------------------------------------

using System;
using System.IO;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.DataStruct;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public class DefaultPacketHeaderHelper : IPacketHeaderHelper
    {
        private int m_headerLength = 4;
        
        public int PackageHeaderLength => m_headerLength;

        public virtual IPacketHeader DeserializeHeader(Stream source, out object customErrorData)
        {
            int headerBytesLenth = 0;
            customErrorData = null;
            //
            // byte[] totalLengthBytes = new byte[4];
            // source.Read(totalLengthBytes, 0, 4);
            // int totalLength = BitConverter.ToInt32(totalLengthBytes, 0);
            int totalLength = ReadInt(source,ref headerBytesLenth);
            return new BasicPacketHeader()
            {
                PacketLength = totalLength,
                HeaderByteLength = m_headerLength
            };
        }

        protected int ReadInt(Stream source,ref int headerBytesLength)
        {
            byte[] bytes = new byte[4];
            source.Read(bytes, 0, 4);
            int value = BitConverter.ToInt32(bytes, 0);
            headerBytesLength += 4;
            return value;
        }
    }
}