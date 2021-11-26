using System.IO;
using UnityEngine;
using Yoozoo.Managers.NetworkV2.DataStruct;

namespace Yoozoo.Managers.NetworkV2.Helpers
{
    public abstract class DeserializeDiverterBase : MonoBehaviour
    {
        public abstract bool IsDeserializeContentByHelper(IPacket packet);
    }
}