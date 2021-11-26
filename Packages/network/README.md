# 网络初始化

整个网络层并没有通过单例的方式提供，而是直接以class对象类的方式提供，所以在创建网络层的时候业务层需要自己维护一个单例开管理network网络层,整个网络层需要通过外部驱动update刷新，才能运行send和receive流程，所以需要在合适的地方调用network的update刷新api

```C#
public class NetworkManager
{
    private static NetworkManager m_instance;
    private Network m_network;
    public NetworkManager GetIsntance()
    {
        if(m_instance!=null)return null;
        return m_instance=new NetworkManager();
    }
    NetworkManager()
    {
		m_network=new Network();
    }
    public void Update()
    {
        m_network.Update();
    }
}

```



# 多频道处理

>>网络模块是通过channel频道的模式进行管理，也就是业务层可以创建多个网络频道，比如聊天专用频道，战斗专用频道

```C#
INetworkChannel channel=m_network.CreateNetworkChannel(string name, ServiceType serviceType, INetworkChannelHelper networkChannelHelper);
channel.Connect(IPAddress ip,int port);
channel.Send(new Packet());
channel.Close();
```

所有的关于网络的操作，比如send，connect，close等都是通过channel来操作，也就是一个channell对应一个网络连接。

name:  频道的名字

serviceType:  频道的类型，现在提供tcp异步类型和tcp同步类型

INetworkChannelHelper: 频道对应的帮助函数，这个主要是处理协议的序列化和反序列化处理

INetworkChannel：创建频道后返回的网络频道对象，可用于做connect和send操作



# 接口详细描述

```C#
    /// <summary>
    /// 网络频道辅助器接口。
    /// </summary>
    public interface INetworkChannelHelper
    {
        /// <summary>
        /// 获取消息包头长度。这个主要是协议包的头长，一版返回4字节，也就是int的长度
        /// </summary>
        int PacketHeaderLength
        {
            get;
        }

        /// <summary>
        /// 初始化网络频道辅助器。
        /// </summary>
        /// <param name="networkChannel">网络频道。</param>
        void Initialize(INetworkChannel networkChannel);

        /// <summary>
        /// 关闭并清理网络频道辅助器。
        /// </summary>
        void Shutdown();

        /// <summary>
        /// 准备进行连接。
        /// </summary>
        void PrepareForConnecting();

        /// <summary>
        /// 发送心跳消息包。在此方法中可以发送心跳协议给服务器
        /// </summary>
        /// <returns>是否发送心跳消息包成功。</returns>
        bool SendHeartBeat();

        /// <summary>
        /// 序列化消息包。
        /// </summary>
        /// <param name="packet">要序列化的消息包。Packet是一个结构体，创建是不会产生GC，主要是包含msgId，和message对象，由业务层传入</param>
        /// <param name="destination">要序列化的目标流。对packet进行二进制操作写入destination流，注意destination.length要设置成指定的发送长度</param>
        /// <returns>是否序列化成功。</returns>
        bool Serialize(Packet packet, Stream destination);

        /// <summary>
        /// 反序列消息包头。
        /// </summary>
        /// <param name="source">要反序列化的来源流。这个仅处理消息的包头信息</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包头。</returns>
        PacketHeader DeserializePacketHeader(Stream source, out object customErrorData);

        /// <summary>
        /// 反序列化消息包。
        /// </summary>
        /// <param name="packetHeader">消息包头。主要是提供body包体的长度</param>
        /// <param name="source">要反序列化的来源流。</param>
        /// <param name="customErrorData">用户自定义错误数据。</param>
        /// <returns>反序列化后的消息包。</returns>
        Packet DeserializePacket(PacketHeader packetHeader, Stream source, out object customErrorData);
    }
```

