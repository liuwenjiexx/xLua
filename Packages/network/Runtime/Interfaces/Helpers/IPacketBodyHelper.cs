using System.IO;
using Yoozoo.Managers.NetworkV2.DataStruct;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public interface IPacketBodyHelper
    {
        /// <summary>
        /// 核心内容解析部分，如初包长度外的其他包头部分
        /// </summary>
        /// <param name="header"></param>
        /// <param name="source"></param>
        /// <param name="customErrorData"></param>
        /// <returns></returns>
        IPacket DeserializePacketCore(IPacketHeader header,Stream source,out object customErrorData);
        
        /// <summary>
        /// 实际协议内容数据部分，如protobuf数据部分
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="customErrorData"></param>
        /// <returns></returns>
        IPacket DeserializePacketContent(IPacket packet,out object customErrorData);
    }
}