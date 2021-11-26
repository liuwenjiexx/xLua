# UMT Network



##  1. 内容简述

----

* 支持的功能

	1. Channel 多通道
	   1. 游戏可以同时存在多个不同的网络连接，如A连接使用TCP+Protobuf，B连接使用 KCP + Json等自由组合
	2. 自定义协议及包头
	   1. 提供了包头的处理类，继承后可选择通道使用的处理策略
	3. 连接上层尽量无感
	   1. 上层只关心当前channel的状态，尽量不需要上层指导连接的状态。
	4. 断线重连的扩展支持
	   1. 提供了断线重连的底层实现，并提供了重连发生时业务层可以实现的扩展。
	
* 设计思路推荐原型

  ![原型图](images\2.jpg)

## 2. 使用步骤

----

## C# 快速使用

1. 在入口场景新建一个gameObject
2. 挂载NetChannel组件，确保autoSetup打开
3. 修改gameObject的name为你希望这个网络通道的名称假设为`channelA`
4. 为netChannel选择你需要使用的Helper
   1. ![image-20210105143603271](images\20190307212349169.png)
5. 调用`UMTNetwork.StartUp("channelA")`来开启这条通道。
6. 代码中使用`var channelA= UMTNetwork.GetChannel("channelA"); channelA.SendByte(new byte[]{})` 或者 `UMTNetwork.SendBytes("channelA",new byte[]{})` 来发送已经序列化的数据。或者使用`channelA.Send(protoData)``UMTNetwork.Send(protoData)`来发送未序列化对象，未序列化对象会被你选择的helper来进行编码。
7. 接收时采用下面代码来处理，其中`rawData`为未使用helper反序列化的原始数据，`message`为helper已解码反序列化好的对象。

```c#
   m_channel.OnNetworkChannelReceivePacket+= OnNetworkChannelReceivePacket;
   private void OnNetworkChannelReceivePacket(NetChannel channel, IPacket packet)
   {
       //数据已被解析
       if (packet.rawData == null && packet.message != null)
       {
           Debug.Log($"Receive Package ${packet.Id} On C#, do your custom Business here!");
       }
   }
```

## Lua的支持

1. 在LuaModule中已经提供BaseNetChannelHandler 和 NetMgr ，其中如果直接操作通道可以使用`BaseNetChannelHandler.new("channelA",LuaProtoHelperInstance)`。UMT也为单通道游戏提供了常用`NetMgr 用来封装`BaseNetChannelHandler`,NetMgr默认使用的是名为`mainChannel`的通道，请确保存在该通道。

2. 发送消息和接收消息可以参考下面几种方式

   ```lua
   NetMgr = require 'Core.NetMgr'
   Proto = require 'LuaModules.LuaProtoBuf.LuaProtoBuf'
Proto:initialize({
   	bytes = C_LuaManager.ReadByteFiles("Proto/Protocal.bytes"),
   	CSCmd = require("Proto.ProtoCSCmd"),
   	SCCmd = require("Proto.ProtoSCCmd")
   })
   
   NetMgr:initialize(Proto)
   
   NetMgr:addNetStateHandler(function(state) 
           if(state==1) 
               print("connected") 
           end 
       end, true)
   NetMgr:addListener("ErrorInfo",function(data) 
           print("Recive An Error Message "....data.error_code) 
       end))
   
   NetMgr:connect("192.168.0.2", "8081")
   
   ---目前callback为使用messageId对应，相同messageId认为为callback，如果你有其他字段可用于确定业务回调，如Order，则需要继承BaseNetChannelHelper来使用
   NetMgr:sendMessage("ActorLevelUpRequest",{ actor_guid == "" ,level_to_up = 1 },function(data) end )
   ```
   
   ```LUA
   local mainChannel = BaseNetChannelHandler.new("mainChannel",proto)
   
   mainChannel.IP = "192.168.0.2";
   mainChannel.Port = "8081";
   mainChannel:StartUp()
   
   mainChannel:addNetStateHandler(function(state) 
           if(state==1) 
               print("connected") 
           end 
       end, true)
   mainChannel:addListener("ErrorInfo",function(data) 
           print("Recive An Error Message "....data.error_code) 
       end))
   
   mainChannel:connect("192.168.0.2", "8081")
   
   ---目前callback为使用messageId对应，相同messageId认为为callback，如果你有其他字段可用于确定业务回调，如Order，则需要继承BaseNetChannelHelper来使用
   mainChannel:sendMessage("ActorLevelUpRequest",{ actor_guid == "" ,level_to_up = 1 },function(data) end )
   
   ```

### 关于Helper

| Helper 接口             | 推荐基类                      | 功能介绍                                                     |
| ----------------------- | ----------------------------- | ------------------------------------------------------------ |
| IPacketHeaderHelper     |                               | 协议包头长度部分解析（必须）                                 |
| IPacketBodyHelper       | -                             | 协议包头其他未解析部分解析，包体内容解析                     |
| IPacketSerializedHelper | -                             | 协议内容解析部分SerializePacketHandler收到包后必然在子线程触发，SerializeContentHandler是否触发由分流器DeserializeDiverterBase决定 |
| IHeartBeatHelper        | DefaultChannelHeartBeatHelper | 定时心跳，SendHeartBeat会被定时触发，可以实现心跳协议发送逻辑 |
| IReconnectHelper        | DefaultReconnectHelper        | 重连支持，当心跳丢失或断线时会触发CheckReconnectAbleOnLoseConnection或CheckReconnectAbleOnLoseHeartBeat。返回值为是否重连。 |
| DeserializeDiverterBase | -                             | 分流策略，IsDeserializeContentByHelper返回值决定该消息是否在网络子线程解析。如luaProtobuf的协议一般返回false，lua中在主线程解析。 |

示例工程已经提供了几个Example的Helper。如果需要可以在其基础上实现自己的Helper。

1. ExamplePackSerializerHelper
2. ExamplePackDeserializerHelper
3. ExampleHeartBeatHelper
4. ExampleReconnectionHelper
   

*** 注意，你可以一个脚本对象实现前三个interface，内部创建已经保证了一个helper的类实例只有一个 ***





## 3. 其他

其他使用疑问请联系wwei@yoozoo.com @战斗法师甘道夫 或 liqi@yoozoo.com @天奇