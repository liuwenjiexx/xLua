using System.IO;
using Yoozoo.Managers.NetworkV2.DataStruct;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public interface IPacketHeaderHelper
    {
        int PackageHeaderLength { get; }

        IPacketHeader DeserializeHeader(Stream source,out object customErrorData);
    }
}