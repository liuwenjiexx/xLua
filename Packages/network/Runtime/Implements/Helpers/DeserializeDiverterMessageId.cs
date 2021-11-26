//-----------------------------------------------------------------------
// Created By 甘道夫
// contact E-mail: wwei@yoozoo.com
// Date: 2020-12-31
// 消息ID的分流器
//-----------------------------------------------------------------------


using System;
using UnityEngine;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.Helpers;

namespace Packages.network.Runtime.V2.Runtime.Helpers
{
    public class DeserializeDiverterMessageId : DeserializeDiverterBase 
    {
        [Serializable]
        public struct Range
        {
            public float min;
            public float max;
        }


        [Tooltip("闭区间，区间内都将使用指定的BodyHelper来解析")]
        [SerializeField]
        public Range[] deserializeMsgIds;
        
        public override bool IsDeserializeContentByHelper(IPacket packet)
        {
            for (int i = 0; i < deserializeMsgIds.Length; i++)
            {
                if (packet.Id >= deserializeMsgIds[i].min && packet.Id <= deserializeMsgIds[i].max)
                {
                    return true;
                }
            }
            return false;
        }
    }
}