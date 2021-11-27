using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yoozoo.Managers.NetworkV2.Core;

public static class NewBehaviourScript  
{

    public static void OnNetworkChannelConnected(NetChannel channel, string opr)
    {
        channel.OnNetworkChannelConnected += (NetChannel channel2, object data) =>
        {


        };
    }


}
