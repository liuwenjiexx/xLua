using System;
using System.Collections;
using UnityEngine;
using Yoozoo.Managers.NetworkV2;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.Enums;
using Yoozoo.Managers.V2.Core;

namespace Yoozoo.Managers.NetworkV2.Examples
{
    public class NetExample : MonoBehaviour
    {
        [SerializeField] private GameObject testChannel;
        
        private NetChannel mainChannel;
        private IEnumerator Start()
        {
            yield return null;
            mainChannel = NetworkEntry.GetChannel(testChannel.gameObject.name);
            
            mainChannel.OnNetworkChannelClosed+= ChannelOnOnNetworkChannelClosed;
            mainChannel.OnNetworkChannelConnected+= ChannelOnOnNetworkChannelConnected;
            mainChannel.OnNetworkChannelError+= ChannelOnOnNetworkChannelError;
            mainChannel.OnNetworkChannelReceivePacket += OnNetworkChannelReceivePacket;
        }

       


        // private void OnGUI()
        // {
        //     if (GUILayout.Button("连接测试"))
        //     {
        //         UMTNetwork.StartUp(testChannel.gameObject.name);
        //     }
        //     
        //     if (GUILayout.Button("断开测试"))
        //     {
        //         UMTNetwork.ShutDown(testChannel.gameObject.name);
        //     }
        //     
        //     if (GUILayout.Button("发送信息"))
        //     {
        //         var bytes = new byte[]
        //         {10,13,49,46,48,46,55,53,50,55,46,51,49,56,57,34,4,102,115,100,102,66,14,49,46,48,46,55,53,52,56,46,49,56,49,57,54,74,17,41,40,85,70,42,38,68,42,38,94,83,70,68,60,83,63,76,18,23,49,49,49,230,156,141,32,32,229,141,149,233,170,145,230,149,145,228,184,150,228,184,187,58,6,49,50,52,50,51,52,50,7,65,110,100,114,111,105,100,26,8,73,95,65,77,95,71,79,68};
        //
        //         UMTNetwork.SendBytes(testChannel.gameObject.name,10001,bytes);
        //     }
        // }

        private void ChannelOnOnNetworkChannelError(NetChannel channel, NetworkErrorCode errortype, int errorcode, string errormsg)
        {
            Debug.Log(errormsg);
        }

        private void ChannelOnOnNetworkChannelConnected(NetChannel channel, object data)
        {
            Debug.Log("[Connected]"+channel.Target);
        }

        private void ChannelOnOnNetworkChannelClosed(NetChannel channel)
        {
            Debug.Log("[DisConnected]"+channel.Target);
        }

        private void OnNetworkChannelReceivePacket(NetChannel channel, IPacket packet)
        {
            Debug.Log("[Receive]"+channel.Target+"/"+packet.Id);
        }
    }
}