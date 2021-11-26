using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Yoozoo.Managers.NetworkV2.Core;
using Yoozoo.Managers.NetworkV2.DataStruct;
using Yoozoo.Managers.NetworkV2.Enums;
using Yoozoo.Managers.NetworkV2.States;

namespace Yoozoo.Managers.NetworkV2.Client
{
    public class TCPSocketClient : INetClient
    {
        private enum CMD_TYPE
        {
            connected,
            close,
            error,
        }

        private struct ErrorParams
        {
            public NetworkErrorCode errorType;
            public int errorCode;
            public string errorMsg;
        }

        private struct CMD
        {
            public CMD_TYPE type;
            public object customData;
        }


        protected readonly Queue<IPacket> m_SendPacketPool = new Queue<IPacket>();
        protected readonly Queue<IPacket> m_SendCurPacketPool = new Queue<IPacket>();
        protected readonly Queue<IPacket> m_ReceivePacketPool = new Queue<IPacket>();
        protected Queue<IPacket> m_SendPacketTemp = new Queue<IPacket>();


        private Queue<CMD> m_commandLine = new Queue<CMD>();

        protected int m_SentPacketCount = 0;
        protected int m_ReceivedPacketCount = 0;


        protected AddressFamilyType m_AddressFamily;

        protected Socket m_Socket;


        private bool m_Active;


        public event NetDelegates.NetClientDelegates.NetClientConnectedHandler OnNetClientConnected;
        public event NetDelegates.NetClientDelegates.NetClientErrorHandler OnNetClientError;
        public event NetDelegates.NetClientDelegates.NetClientClosedHandler OnNetClientClosed;
        public event NetDelegates.NetClientDelegates.NetClientReceivePacket OnNetClientReceivePacket;
        public NetDelegates.HelperDelegates.DeserializeHeaderHandler deserializeHeaderHandler { get; set; }
        public NetDelegates.HelperDelegates.DeserializePacketHandler deserializePacketHandler { get; set; }
        public NetDelegates.HelperDelegates.SerializeHandler serializePacketHandler { get; set; }
        public NetDelegates.HelperDelegates.SerializeHandler serializeContentHandler { get; set; }


        protected readonly NetClient.SendState m_SendState = new NetClient.SendState();
        protected readonly NetClient.ReceiveState m_ReceiveState = new NetClient.ReceiveState();


        public int PacketHeaderLength { get; set; }
        public bool AutoResendFailedPacket { get; set; } = false;


        #region Send & Recive Info

        /// <summary>
        /// 获取要发送的消息包数量。
        /// </summary>
        public int SendPacketCount
        {
            get { return m_SendPacketPool.Count; }
        }

        /// <summary>
        /// 获取累计发送的消息包数量。
        /// </summary>
        public int SentPacketCount
        {
            get { return m_SentPacketCount; }
        }

        /// <summary>
        /// 获取已接收未处理的消息包数量。
        /// </summary>
        public int ReceivePacketCount
        {
            get { return m_ReceivePacketPool.Count; }
        }

        /// <summary>
        /// 获取累计已接收的消息包数量。
        /// </summary>
        public int ReceivedPacketCount
        {
            get { return m_ReceivedPacketCount; }
        }

        #endregion


        public AddressFamilyType addressFamily
        {
            get => m_AddressFamily;
            set { m_AddressFamily = value; }
        }

        public void Connect(IPAddress ipAddress, int port, object userData)
        {
            if (m_Socket != null)
            {
                Close();
                m_Socket = null;
            }

            switch (ipAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    m_AddressFamily = AddressFamilyType.IPv4;
                    break;

                case AddressFamily.InterNetworkV6:
                    m_AddressFamily = AddressFamilyType.IPv6;
                    break;

                default:
                    string errorMessage = string.Format("Not supported address family '{0}'.",
                        ipAddress.AddressFamily.ToString());
                    if (OnNetClientError != null)
                    {
                        NetError(this, NetworkErrorCode.AddressFamilyError, (int) SocketError.Success,
                            errorMessage);
                        return;
                    }

                    throw new Exception(errorMessage);
            }

            m_SendState.Reset();
            m_ReceiveState.PrepareForPacketHeader(PacketHeaderLength);


            m_Socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (m_Socket == null)
            {
                string errorMessage = "Initialize network channel failure.";
                if (OnNetClientError != null)
                {
                    NetError(this, NetworkErrorCode.SocketError, (int) SocketError.Success, errorMessage);
                    return;
                }

                throw new Exception(errorMessage);
            }

            ConnectAsync(ipAddress, port, userData);
        }


        public void Close()
        {
            lock (this)
            {
                if (m_Socket == null)
                {
                    return;
                }

                m_Active = false;

                try
                {
                    m_Socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
                finally
                {
                    m_Socket.Close();
                    m_Socket = null;

                    // if (OnNetClientClosed != null)
                    // {
                    //     OnNetClientClosed(this);
                    // }
                    lock (m_commandLine)
                    {
                        m_commandLine.Enqueue(new CMD
                        {
                            type = CMD_TYPE.close
                        });
                    }
                }

                m_SentPacketCount = 0;
                m_ReceivedPacketCount = 0;

                lock (m_SendPacketPool)
                {
                    m_SendPacketPool.Clear();
                }

                m_ReceivePacketPool.Clear();

                // lock (m_HeartBeatState)
                // {
                //     m_HeartBeatState.Reset(true);
                // }
            }
        }


        private void ConnectAsync(IPAddress ipAddress, int port, object userData)
        {
            try
            {
                m_Socket.BeginConnect(ipAddress, port, ConnectCallback, new TCPClient.ConnectState(m_Socket, userData));
            }
            catch (Exception exception)
            {
                m_Active = false;
                if (OnNetClientError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetError(this, NetworkErrorCode.ConnectError,
                        (int) (socketException != null ? socketException.SocketErrorCode : SocketError.Success),
                        exception.ToString());
                    return;
                }

                throw;
            }
        }


        private void ConnectCallback(IAsyncResult ar)
        {
            TCPClient.ConnectState socketUserData = (TCPClient.ConnectState) ar.AsyncState;
            try
            {
                socketUserData.Socket.EndConnect(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception exception)
            {
                m_Active = false;
                if (OnNetClientError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetError(this, NetworkErrorCode.ConnectError,
                        (int) (socketException != null ? socketException.SocketErrorCode : SocketError.Success),
                        exception.ToString());
                    return;
                }

                throw;
            }

            m_SentPacketCount = 0;
            m_ReceivedPacketCount = 0;

            lock (m_SendPacketPool)
            {
                m_SendPacketPool.Clear();
            }

            m_ReceivePacketPool.Clear();


            m_Active = true;
            // if (OnNetClientConnected != null)
            // {
            //     OnNetClientConnected(this, socketUserData.UserData);
            // }
            lock (m_commandLine)
            {
                m_commandLine.Enqueue(new CMD
                {
                    type = CMD_TYPE.connected,
                    customData = socketUserData.UserData
                });
            }

            ReceiveAsync();
        }


        private void NetError(INetClient client, NetworkErrorCode errorType, int errorCode, string errorMsg)
        {
            lock (m_commandLine)
            {
                m_commandLine.Enqueue(new CMD
                {
                    type = CMD_TYPE.error,
                    customData = new ErrorParams
                    {
                        errorType = errorType,
                        errorCode = errorCode,
                        errorMsg = errorMsg
                    }
                });
            }
        }

        public void Update()
        {
            lock (m_commandLine)
            {
                int cmdCnt = m_commandLine.Count;
                for (int i = 0; i < cmdCnt; i++)
                {
                    var cmd = m_commandLine.Dequeue();
                    switch (cmd.type)
                    {
                        case CMD_TYPE.connected:
                            OnNetClientConnected?.Invoke(this, cmd.customData);
                            break;
                        case CMD_TYPE.close:
                            OnNetClientClosed?.Invoke(this);
                            break;
                        case CMD_TYPE.error:
                            var errorParams = (ErrorParams) cmd.customData;
                            OnNetClientError?.Invoke(this, errorParams.errorType, errorParams.errorCode,
                                errorParams.errorMsg);
                            break;
                    }
                }
            }


            if (m_Socket == null || !m_Active)
            {
                return;
            }

            ProcessSend();
            ProcessReceive();

            if (m_Socket == null || !m_Active)
            {
                return;
            }

            int count = m_ReceivePacketPool.Count;
            for (int i = 0; i < count; i++)
            {
                var o = m_ReceivePacketPool.Dequeue();
                OnNetClientReceivePacket?.Invoke(this, o);
            }
        }

        public bool IsAvailable => (m_Socket != null && m_Active);


        #region Send & Recive

        /// <summary>
        /// 向远程主机发送消息包。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="packet">要发送的消息包。</param>
        public void Send(IPacket packet)
        {
            if (m_Socket == null)
            {
                string errorMessage = "You must connect first.";
                if (OnNetClientError != null)
                {
                    NetError(this, NetworkErrorCode.SendError, (int) SocketError.Success, errorMessage);
                    return;
                }

                throw new Exception(errorMessage);
            }

            if (!m_Active)
            {
                string errorMessage = "Socket is not active.";
                if (OnNetClientError != null)
                {
                    NetError(this, NetworkErrorCode.SendError, (int) SocketError.Success, errorMessage);
                    return;
                }

                throw new Exception(errorMessage);
            }

            lock (m_SendPacketPool)
            {
                m_SendPacketPool.Enqueue(packet);
            }
        }

        private void SendAsync()
        {
            try
            {
                m_Socket.BeginSend(m_SendState.Stream.GetBuffer(), (int) m_SendState.Stream.Position,
                    (int) (m_SendState.Stream.Length - m_SendState.Stream.Position), SocketFlags.None, SendCallback,
                    m_Socket);
            }
            catch (Exception exception)
            {
                m_Active = false;
                if (OnNetClientError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetError(this, NetworkErrorCode.SendError,
                        (int) (socketException != null ? socketException.SocketErrorCode : SocketError.Success),
                        exception.ToString());
                    return;
                }

                throw;
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket socket = (Socket) ar.AsyncState;
            if (!socket.Connected)
            {
                return;
            }

            int bytesSent = 0;
            try
            {
                bytesSent = socket.EndSend(ar);
            }
            catch (Exception exception)
            {
                m_SendState.NeedToResend(true);
                m_Active = false;
                if (OnNetClientError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetError(this, NetworkErrorCode.SendError,
                        (int) (socketException != null ? socketException.SocketErrorCode : SocketError.Success),
                        exception.ToString());
                    return;
                }

                throw;
            }

            m_SendState.Stream.Position += bytesSent;
            if (m_SendState.Stream.Position < m_SendState.Stream.Length)
            {
                SendAsync();
                return;
            }

            m_SentPacketCount++;
            m_SendState.Reset();
        }

        private void ReceiveAsync()
        {
            try
            {
                m_Socket.BeginReceive(m_ReceiveState.Stream.GetBuffer(), (int) m_ReceiveState.Stream.Position,
                    (int) (m_ReceiveState.Stream.Length - m_ReceiveState.Stream.Position), SocketFlags.None,
                    ReceiveCallback, m_Socket);
            }
            catch (Exception exception)
            {
                m_Active = false;
                if (OnNetClientError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetError(this, NetworkErrorCode.ReceiveError,
                        (int) (socketException != null ? socketException.SocketErrorCode : SocketError.Success),
                        exception.ToString());
                    return;
                }

                throw;
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket) ar.AsyncState;
            if (!socket.Connected)
            {
                return;
            }

            int bytesReceived = 0;
            try
            {
                bytesReceived = socket.EndReceive(ar);
            }
            catch (Exception exception)
            {
                m_Active = false;
                if (OnNetClientError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetError(this, NetworkErrorCode.ReceiveError,
                        (int) (socketException != null ? socketException.SocketErrorCode : SocketError.Success),
                        exception.ToString());
                    return;
                }

                throw;
            }

            if ((m_ReceiveState.Stream.Length - m_ReceiveState.Stream.Position)>0 && bytesReceived <= 0)
            {
                Close();
                return;
            }

            m_ReceiveState.Stream.Position += bytesReceived;
            if (m_ReceiveState.Stream.Position < m_ReceiveState.Stream.Length)
            {
                ReceiveAsync();
                return;
            }

            m_ReceiveState.Stream.Position = 0L;

            bool processSuccess = false;
            if (m_ReceiveState.PacketHeader != null)
            {
                processSuccess = ProcessPacket();
                m_ReceivedPacketCount++;
            }
            else
            {
                processSuccess = ProcessPacketHeader();
            }

            if (processSuccess)
            {
                ReceiveAsync();
                return;
            }
        }

        #endregion


        #region Processes

        protected virtual bool ProcessSend()
        {
            if (m_SendState.Stream.Length > 0 || m_SendPacketPool.Count <= 0)
            {
                return false;
            }

            m_SendPacketTemp.Clear();
            if (AutoResendFailedPacket && m_SendState.NeedResend)
            {
                while (m_SendCurPacketPool.Count > 0)
                {
                    lock (m_SendCurPacketPool)
                    {
                        m_SendPacketTemp.Enqueue(m_SendCurPacketPool.Dequeue());
                    }
                }

                m_SendState.NeedToResend(false);
            }
            else
            {
                m_SendCurPacketPool.Clear();
                while (m_SendPacketPool.Count > 0)
                {
                    lock (m_SendPacketPool)
                    {
                        m_SendPacketTemp.Enqueue(m_SendPacketPool.Dequeue());
                    }
                }
            }

            while (m_SendPacketTemp.Count > 0)
            {
                IPacket packet;
                packet = m_SendPacketTemp.Dequeue();

                bool serializeResult = false;
                try
                {
                    if (packet.message != null)
                    {
                        serializeResult = serializeContentHandler(packet, m_SendState.Stream);
                        if (serializeResult)
                        {
                            serializeResult = serializePacketHandler(packet, m_SendState.Stream);
                        }
                    }
                    else
                    {
                        serializeResult = serializePacketHandler(packet, m_SendState.Stream);
                    }
                }
                catch (Exception exception)
                {
                    m_Active = false;
                    if (OnNetClientError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetError(this, NetworkErrorCode.SerializeError,
                            (int) (socketException != null ? socketException.SocketErrorCode : SocketError.Success),
                            exception.ToString());
                        return false;
                    }

                    throw;
                }

                if (!serializeResult)
                {
                    string errorMessage = "Serialized packet failure.";
                    if (OnNetClientError != null)
                    {
                        NetError(this, NetworkErrorCode.SerializeError, (int) SocketError.Success,
                            errorMessage);
                        return false;
                    }

                    throw new Exception(errorMessage);
                }

                m_SendCurPacketPool.Enqueue(packet);
            }

            m_SendState.Stream.Position = 0L;

            SendAsync();

            return true;
        }

        protected virtual void ProcessReceive()
        {
        }

        protected bool ProcessPacketHeader()
        {
            try
            {
                object customErrorData = null;

                IPacketHeader packetHeader = deserializeHeaderHandler(m_ReceiveState.Stream, out customErrorData);

                if (packetHeader.PacketLength <= 0)
                {
                    string errorMessage = "Packet header is invalid.";
                    if (OnNetClientError != null)
                    {
                        NetError(this, NetworkErrorCode.DeserializePacketHeaderError,
                            (int) SocketError.Success,
                            errorMessage);
                        return false;
                    }

                    throw new Exception(errorMessage);
                }

                m_ReceiveState.PrepareForPacket(packetHeader);
                if (packetHeader.PacketLength <= 0)
                {
                    bool processSuccess = ProcessPacket();
                    m_ReceivedPacketCount++;
                    return processSuccess;
                }
            }
            catch (Exception exception)
            {
                m_Active = false;
                if (OnNetClientError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetError(this, NetworkErrorCode.DeserializePacketHeaderError,
                        (int) (socketException != null ? socketException.SocketErrorCode : SocketError.Success),
                        exception.ToString());
                    return false;
                }

                throw;
            }

            return true;
        }

        protected virtual bool ProcessPacket()
        {
            // lock (m_HeartBeatState)
            // {
            //     m_HeartBeatState.Reset(m_ResetHeartBeatElapseSecondsWhenReceivePacket);
            // }

            try
            {
                object customErrorData = null;
                var packet = deserializePacketHandler(m_ReceiveState.PacketHeader,
                    m_ReceiveState.Stream, out customErrorData);

                // if (customErrorData != null && NetworkChannelCustomError != null)
                // {
                //     NetworkChannelCustomError(this, customErrorData);
                // }

                m_ReceivePacketPool.Enqueue(packet);

                m_ReceiveState.PrepareForPacketHeader(m_ReceiveState.PacketHeader.HeaderByteLength);
            }
            catch (Exception exception)
            {
                m_Active = false;
                if (OnNetClientError != null)
                {
                    SocketException socketException = exception as SocketException;
                    NetError(this, NetworkErrorCode.DeserializePacketError,
                        (int) (socketException != null ? socketException.SocketErrorCode : SocketError.Success),
                        exception.ToString());
                    return false;
                }

                throw;
            }

            return true;
        }

        #endregion
    }
}