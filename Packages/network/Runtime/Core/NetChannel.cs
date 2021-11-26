using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using Yoozoo.Managers.NetworkV2.Client;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.Enums;
using Yoozoo.Managers.NetworkV2.Helpers;
using Yoozoo.Managers.V2.Core;

namespace Yoozoo.Managers.NetworkV2.Core
{
    public class NetChannel : MonoBehaviour
    {
        [SerializeField] private AddressFamilyType m_addressFamily = AddressFamilyType.IPv4;


        [SerializeField] private string m_IPAddress = "127.0.0.1";

        [SerializeField] private int m_Port = 8080;

        [SerializeField] private ClientTypeEnum clientTypeEnum = ClientTypeEnum.TCP;


        [SerializeField] private bool autoSetup = false;
        
        [Tooltip("是否自动重发发送失败的包，如果你的包头没有order保证重收无误，请不要开启")]
        [SerializeField] private bool autoResendFailedPacket = false;

        [HideInInspector] [SerializeField] private string m_PacketHeaderHelperTypeKey =
            "Yoozoo.Managers.NetworkV2.Helpers.DefaultPacketHeaderHelper";

        [HideInInspector] [SerializeField]
        private string m_PacketBodyHelperTypeKey = "Yoozoo.Managers.NetworkV2.Helpers.DefaultPacketBodyHelper";

        [HideInInspector] [SerializeField] private string m_PacketSerializerHelperTypeKey =
            "Yoozoo.Managers.NetworkV2.Helpers.DefaultPacketSerializerHelper";

        [HideInInspector] [SerializeField]
        private string m_HeartBeatHelperTypeKey = "Yoozoo.Managers.NetworkV2.Helpers.IHeartBeatHelper";

        [HideInInspector] [SerializeField]
        private string m_ReconnectHelperTypeKey = "Yoozoo.Managers.NetworkV2.Helpers.IReconnectHelper";

        private IPacketHeaderHelper m_PacketHeaderHelper;
        private IPacketBodyHelper m_PacketBodyHelper;
        private IPacketSerializedHelper m_PacketSerializerHelper;
        private DeserializeDiverterBase m_DeserializeDiverter;
        private IHeartBeatHelper m_HeartBeatHelper;
        private IReconnectHelper m_ReconnectHelper;

        public string Name => gameObject.name;

        private INetClient m_netClient;

        #region Events

        public event NetDelegates.ChannelDelegates.NetworkChannelConnectedHandler OnNetworkChannelConnected;
        public event NetDelegates.ChannelDelegates.NetworkChannelReconnectedHandler OnNetworkChannelReconnected;
        public event NetDelegates.ChannelDelegates.NetworkChannelClosedHandler OnNetworkChannelClosed;
        public event NetDelegates.ChannelDelegates.NetworkChannelErrorHandler OnNetworkChannelError;
        public event NetDelegates.ChannelDelegates.NetworkChannelReceivePacketHandler OnNetworkChannelReceivePacket;
        
        /// <summary>
        /// 通道开启事件
        /// </summary>
        public event NetDelegates.ChannelDelegates.NetworkChannelStartUpHandler OnNetworkChannelStartUp;
        
        /// <summary>
        /// 通道关闭事件
        /// </summary>
        public event NetDelegates.ChannelDelegates.NetworkChannelShutDownHandler OnNetworkChannelShutDown;

        #endregion


        #region Getters & Setters
        
        
        public string IP
        {
            get => m_IPAddress;
            set
            {
                if (!IsStartUp)
                {
                    m_IPAddress = value;
                }
                else
                {
                    Debug.LogWarning("can't set ip address when channel was startup");
                }
            }
        }

        public int Port
        {
            get => m_Port;
            set
            {
                if (!IsStartUp)
                {
                    m_Port = value;
                }
                else
                {
                    Debug.LogWarning("can't set ip port when channel was startup");
                }
            }
        }

        public string Target => $"{IP}:{Port}";


        public bool IsStartUp { get; set; } = false; 
        public bool IsConnected { get; private set; } = false; 

        #endregion


        private void Awake()
        {
            NetworkEntry.RegisterChannel(this);
            if (autoSetup)
                Setup();
        }

        private float m_lastUpdateTm = 0f;

        private void Update()
        {
            float realDt = Time.realtimeSinceStartup - m_lastUpdateTm;
            m_netClient?.Update();
            m_HeartBeatHelper?.Update(this, m_netClient, realDt);
            m_ReconnectHelper?.Update(this, m_netClient);
            m_lastUpdateTm = Time.realtimeSinceStartup;
            
            
        }


        public void Setup()
        {
            setupClientByType();
        }

        public void StartUp(object connectionUserData = null)
        {
            IsConnected = false;
            if (IsStartUp)
            {
                ShutDownInternal(false);
            }

            IsStartUp = true;
            m_ReconnectHelper?.Reset(this,m_netClient);
            StartUpInternal(connectionUserData);
            OnNetworkChannelStartUp?.Invoke(this);
        }

        internal void StartUpInternal(object connectionUserData = null)
        {
            if (IPAddress.TryParse(m_IPAddress,out var address))
            {
                m_netClient.Connect(address,m_Port, connectionUserData);
            }
            else
            {
                IPHostEntry host = Dns.GetHostEntry(m_IPAddress);
                IPAddress ip = host.AddressList[0];
                m_netClient.Connect(ip,m_Port, connectionUserData);
            }
        }
        public void ShutDown(bool destroySelf = false)
        {
            ShutDownInternal();
            if (destroySelf)
            {
                Destroy(gameObject);
            }
            
        }

        public void TestCloseSocket()
        {
            m_netClient.Close();
        }

        public void ShutDownInternal(bool shutDownByUser = true)
        {
            IsConnected = false;
            IsStartUp = false;
            m_ReconnectHelper?.Reset(this,m_netClient);
            m_netClient.Close();
            
            OnNetworkChannelShutDown?.Invoke(this,shutDownByUser);
        }

        public void SendBytes(int messageID, byte[] messageBytes,object customData = null)
        {
            m_netClient.Send(new BasicPacket()
            {
                Id = messageID,
                rawData = messageBytes,
                customData = customData,
            });
        }

        public void Send(int messageID, object messageData,object customData = null)
        {
            m_netClient.Send(new BasicPacket()
            {
                Id = messageID,
                message = messageData,
                customData = customData
            });
        }

        private void setupClientByType()
        {
            switch (clientTypeEnum)
            {
                case ClientTypeEnum.UDP:
                    m_netClient = new UDPSocketClient();
                    break;
                case ClientTypeEnum.KCP:
                    m_netClient = new KCPSocketClient();
                    break;
                case ClientTypeEnum.WebSocket:
                    m_netClient = new WebSocketClient();
                    break;
                case ClientTypeEnum.TCP:
                default:
                    m_netClient = new TCPSocketClient();
                    break;
            }

            m_netClient.AutoResendFailedPacket = autoResendFailedPacket;
            m_netClient.addressFamily = m_addressFamily;
            m_netClient.OnNetClientError += OnNetClientError;
            m_netClient.OnNetClientConnected += OnNetClientConnected;
            m_netClient.OnNetClientClosed += OnNetClientClosed;
            m_netClient.OnNetClientReceivePacket += OnNetClientReceivePacket;

            m_netClient.deserializeHeaderHandler = DeserializeHeaderHandler;
            m_netClient.deserializePacketHandler = DeserializePacketHandler;
            m_netClient.serializePacketHandler = SerializePacketHandler;
            m_netClient.serializeContentHandler = SerializeContentHandler;

            InitPacketHeaderHelper();
            InitPacketBodyHelper();
            InitPacketSerializerHelper();
            InitPacketDeserializeDiverter();
            InitHeartBeatHelper();
            InitReconnectionHelper();
            m_netClient.PacketHeaderLength = m_PacketHeaderHelper.PackageHeaderLength;
        }


        private bool SerializePacketHandler(IPacket data, Stream source)
        {
            if (m_PacketSerializerHelper != null)
            {
                return m_PacketSerializerHelper.SerializePacketHandler(data, source);
            }
            else
            {
                return false;
            }
        }
        private bool SerializeContentHandler(IPacket data, Stream source)
        {
            if (m_PacketSerializerHelper != null)
            {
                return m_PacketSerializerHelper.SerializeContentHandler(data, source);
            }
            else
            {
                return false;
            }
        }

        private IPacket DeserializePacketHandler(IPacketHeader header, Stream source, out object customErrorData)
        {
            if (m_PacketBodyHelper != null)
            {
                //解出核心部分，比如messageID
                var packet = m_PacketBodyHelper.DeserializePacketCore(header, source, out customErrorData);
                packet.header = header;
                //如果存在分流器，且分流器检测不需要Helper来解析则直接跳过内容解析部分
                if (m_DeserializeDiverter != null && !m_DeserializeDiverter.IsDeserializeContentByHelper(packet))
                {
                    return packet;
                }

                packet =  m_PacketBodyHelper.DeserializePacketContent(packet, out customErrorData);
                packet.rawData = null;
                return packet;
            }
            else
            {
                customErrorData = null;
                return default(IPacket);
            }
        }

        private IPacketHeader DeserializeHeaderHandler(Stream source, out object customErrorData)
        {
            if (m_PacketHeaderHelper != null)
            {
                return m_PacketHeaderHelper.DeserializeHeader(source, out customErrorData);
            }
            else
            {
                customErrorData = null;
                return default(IPacketHeader);
                ;
            }
        }

        private void OnNetClientClosed(INetClient client)
        {
            OnNetworkChannelClosed?.Invoke(this);
            if (IsStartUp && IsConnected)
            {
                m_ReconnectHelper?.OnLoseConnections(this,client);
            }
        }

        private void OnNetClientConnected(INetClient client, object data)
        {
            IsConnected = true;
            m_PacketSerializerHelper?.ResetOnConnected(this,client,data);
            m_HeartBeatHelper?.ResetHeartBeat(false);
            OnNetworkChannelConnected?.Invoke(this, data);
            m_ReconnectHelper?.OnConnectionSuccess(this,client,data);
        }

        private void OnNetClientError(INetClient client, NetworkErrorCode errortype, int errorcode, string errormsg)
        {
            OnNetworkChannelError?.Invoke(this, errortype, errorcode, errormsg);
            
            if (IsStartUp)
            {
                if (IsConnected)
                {
                    if( errortype == NetworkErrorCode.ConnectError 
                         || errortype == NetworkErrorCode.SocketError
                         || errortype == NetworkErrorCode.SendError
                         || errortype == NetworkErrorCode.ReceiveError)
                        m_ReconnectHelper.OnLoseConnections(this, client);
                }
                else
                {
                    this.ShutDown();
                }
            }
        }

        private void OnNetClientReceivePacket(INetClient client, IPacket packet)
        {
            m_HeartBeatHelper?.ResetHeartBeat(true);
            OnNetworkChannelReceivePacket?.Invoke(this, packet);
        }
        
        private void OnHeartBeatHelperMissHeartBeat(int count)
        {
            m_ReconnectHelper?.OnLoseHeartBeat(this,m_netClient , count);
        }

        #region Helper Init

        private Dictionary<string, object> m_HelperPool = new Dictionary<string, object>();

        private T GetFromHelperPool<T>(string className)
        {
            bool found = false;
            if (m_HelperPool.ContainsKey(className))
            {
                found = true;
                var cachedHelper = m_HelperPool[className];
                if (cachedHelper is T)
                {
                    return (T) cachedHelper;
                }
            }

            Type helperType = Utility.Assembly.GetType(className);
            if (helperType == null)
            {
                throw new Exception("Can not find GetFromHelperPool '{className}'.");
            }

            T helper = (T) Activator.CreateInstance(helperType);
            if (helper == null)
            {
                throw new Exception("Can not create packetHeader helper instance '{className}'.");
            }

            if (!found)
                m_HelperPool.Add(className, helper);
            else
                m_HelperPool[className] = helper;
            return helper;
        }

        private void InitPacketHeaderHelper()
        {
            if (string.IsNullOrEmpty(m_PacketHeaderHelperTypeKey))
            {
                return;
            }

            m_PacketHeaderHelper = GetFromHelperPool<IPacketHeaderHelper>(m_PacketHeaderHelperTypeKey);
        }

        private void InitPacketBodyHelper()
        {
            if (string.IsNullOrEmpty(m_PacketBodyHelperTypeKey))
            {
                return;
            }

            m_PacketBodyHelper = GetFromHelperPool<IPacketBodyHelper>(m_PacketBodyHelperTypeKey);
        }

        private void InitPacketDeserializeDiverter()
        {
            DeserializeDiverterBase deserializeDiverterBase = GetComponent<DeserializeDiverterBase>();
            m_DeserializeDiverter = deserializeDiverterBase;
        }

        private void InitPacketSerializerHelper()
        {
            if (string.IsNullOrEmpty(m_PacketSerializerHelperTypeKey))
            {
                return;
            }

            m_PacketSerializerHelper = GetFromHelperPool<IPacketSerializedHelper>(m_PacketSerializerHelperTypeKey);
        }

        private void InitHeartBeatHelper()
        {
            if (string.IsNullOrEmpty(m_HeartBeatHelperTypeKey))
            {
                return;
            }

            m_HeartBeatHelper = GetFromHelperPool<IHeartBeatHelper>(m_HeartBeatHelperTypeKey);
            m_HeartBeatHelper.onMissHeartBeat += OnHeartBeatHelperMissHeartBeat;
            m_HeartBeatHelper.Init(this);
        }



        private void InitReconnectionHelper()
        {
            if (string.IsNullOrEmpty(m_ReconnectHelperTypeKey))
            {
                return;
            }

            m_ReconnectHelper = GetFromHelperPool<IReconnectHelper>(m_ReconnectHelperTypeKey);
            m_ReconnectHelper.OnReconnected += OnReconnectHelperReconnected;

        }

        private void OnReconnectHelperReconnected()
        {
            OnNetworkChannelReconnected?.Invoke(this);
        }

        #endregion
    }
}