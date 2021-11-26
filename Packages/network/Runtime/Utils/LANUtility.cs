using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Yoozoo.Managers.NetworkV2.Enums;

namespace Yoozoo.Managers.NetworkV2.Core
{
    public class LANUtility
    {
        /// <summary>
        /// 获取本机IP
        /// </summary>
        /// <param name="Addfam">要获取的IP类型</param>
        /// <returns></returns>
        public static List<string> GetIP(AddressFamilyType addressFamily)
        {
            if (addressFamily == AddressFamilyType.IPv6 && !Socket.OSSupportsIPv6)
            {
                return null;
            }

            List<string> output = new List<string>();

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        //IPv4
                        if (addressFamily == AddressFamilyType.IPv4)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                output.Add(ip.Address.ToString());
                            }
                        }

                        //IPv6
                        else if (addressFamily == AddressFamilyType.IPv6)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                output.Add(ip.Address.ToString());
                            }
                        }
                    }
                }
            }
            return output;
        }
        
        
        
        
        /// <summary> 
        /// Check if startPort is available, incrementing and 
        /// checking again if it's in use until a free port is found 
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static int FindNextAvailableTCPPort(int startPort) 
        { 
            int port = startPort; 
            bool isAvailable = true;

            var mutex = new Mutex(false, 
                string.Concat("Global/", PortReleaseGuid)); 
            mutex.WaitOne(); 
            try 
            { 
                IPGlobalProperties ipGlobalProperties = 
                    IPGlobalProperties.GetIPGlobalProperties(); 
                IPEndPoint[] endPoints = 
                    ipGlobalProperties.GetActiveTcpListeners();

                do 
                { 
                    if (!isAvailable) 
                    { 
                        port++; 
                        isAvailable = true; 
                    }

                    foreach (IPEndPoint endPoint in endPoints) 
                    { 
                        if (endPoint.Port != port) continue; 
                        isAvailable = false; 
                        break; 
                    }

                } while (!isAvailable && port < IPEndPoint.MaxPort);

                if (!isAvailable) 
                    throw new Exception("Not able to find a free TCP port.");

                return port; 
            } 
            finally 
            { 
                mutex.ReleaseMutex(); 
            } 
        }

        
        private const string PortReleaseGuid = "8875BD8E-4D5B-11DE-B2F4-691756D89593";
        /// <summary> 
        /// Check if startPort is available, incrementing and 
        /// checking again if it's in use until a free port is found 
        /// </summary> 
        /// <param name="startPort">The first port to check</param> 
        /// <returns>The first available port</returns> 
        public static int FindNextAvailableUDPPort(int startPort) 
        { 
            int port = startPort; 
            bool isAvailable = true;

            var mutex = new Mutex(false, 
                string.Concat("Global/", PortReleaseGuid)); 
            mutex.WaitOne(); 
            try 
            { 
                IPGlobalProperties ipGlobalProperties = 
                    IPGlobalProperties.GetIPGlobalProperties(); 
                IPEndPoint[] endPoints = 
                    ipGlobalProperties.GetActiveUdpListeners();

                do 
                { 
                    if (!isAvailable) 
                    { 
                        port++; 
                        isAvailable = true; 
                    }

                    foreach (IPEndPoint endPoint in endPoints) 
                    { 
                        if (endPoint.Port != port) 
                            continue; 
                        isAvailable = false; 
                        break; 
                    }

                } while (!isAvailable && port < IPEndPoint.MaxPort);

                if (!isAvailable) 
                    throw new Exception("Not able to find a free TCP port.");

                return port; 
            } 
            finally 
            { 
                mutex.ReleaseMutex(); 
            } 
        } 
    }
}