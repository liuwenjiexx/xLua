using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Yoozoo.Managers.NetworkV2.Enums;

namespace Yoozoo.Managers.NetworkV2.Core
{
    public class LANBroadcaster : MonoBehaviour
    {
        [SerializeField] public int port = 7799;
        
        private UdpClient m_UDPRecv;
        private IPEndPoint m_endPointRecv;

        private UdpClient m_UDPSend;
        private List<IPEndPoint> m_endPointSend = new List<IPEndPoint>();
        private Thread broadThread;

        
        private Queue<byte[]> m_waitSendBuffs = new Queue<byte[]>();
        private Queue<BroadcastPacket> m_recvBuffs = new Queue<BroadcastPacket>();

        struct BroadcastPacket
        {
            public string IP;
            public int Port;
            public byte[] RawData;
        }


        public delegate void OnRecvBoardCastHandler(string ip,int port,string message);


        public event OnRecvBoardCastHandler onReceiveBoardCastMessage;

        void Start()
        {
            m_UDPRecv = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            m_endPointRecv = new IPEndPoint(IPAddress.Any, 0);
            m_UDPRecv.EnableBroadcast = true;
            m_UDPRecv.BeginReceive(ReceiveCallback, null);
            
            m_UDPSend = new UdpClient(new IPEndPoint(IPAddress.Any, 0));


            m_endPointSend.Clear();
            var ips = LANUtility.GetIP(AddressFamilyType.IPv4);
            for (int i = 0; i < ips.Count; i++)
            {
                IPAddress ipAddress =
                    IPAddress.Parse($"{ips[i].Substring(0, ips[i].LastIndexOf(".", StringComparison.Ordinal))}.255");
                m_endPointSend.Add(new IPEndPoint(ipAddress, port));
            }

            broadThread = new Thread(BroadThread);
            broadThread.IsBackground = true;
            broadThread.Start();
        }

        private void OnDestroy()
        {
            broadThread.Abort();
            
            m_UDPSend.Close();
            m_UDPRecv.Close();
            
            m_UDPSend.Dispose();
            m_UDPRecv.Dispose();

            onReceiveBoardCastMessage = null;
        }

        private void Update()
        {
            lock (m_recvBuffs)
            {
                while (m_recvBuffs.Count>0)
                {
                    var packet = m_recvBuffs.Dequeue();
                    var message = Encoding.UTF8.GetString(packet.RawData);
                    onReceiveBoardCastMessage?.Invoke(packet.IP,packet.Port,message);
                }
            }
        }


        public void BroadCast(string message)
        {
            BroadCast(Encoding.UTF8.GetBytes(message));
        }
        
        private void BroadCast(byte[] rawData)
        {
            lock (m_waitSendBuffs)
            {
                m_waitSendBuffs.Enqueue(rawData);
            }
        }


        private void BroadThread()
        {
            while (true)
            {
                lock (m_waitSendBuffs)
                {
                    if (m_waitSendBuffs.Count > 0)
                    {
                        var rawData = m_waitSendBuffs.Dequeue();
                        for (int i = 0; i < m_endPointSend.Count; i++)
                        {
                            try
                            {
                                Debug.Log(m_endPointSend[i].ToString());
                                m_UDPSend.EnableBroadcast = true;
                                m_UDPSend.Send(rawData, rawData.Length, m_endPointSend[i]);
                            }
                            catch (Exception e)
                            {
                                Debug.Log(e.ToString());
                            }
                        }
                       
                    }
                }
                Thread.Sleep(200);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            byte[] bytesReceived;
            try
            {
                bytesReceived = m_UDPRecv.EndReceive(ar, ref m_endPointRecv);
            }
            catch (Exception exception)
            {
                SocketException socketException = exception as SocketException;
                Debug.Log(exception.ToString());
                return;
            }

            lock (m_recvBuffs)
            {
                m_recvBuffs.Enqueue(new BroadcastPacket
                {
                    IP = m_endPointRecv.Address.ToString(),
                    Port =  m_endPointRecv.Port,
                    RawData = bytesReceived
                });
            }
            m_UDPRecv.BeginReceive(ReceiveCallback, null);
        }
        
        
    }
}