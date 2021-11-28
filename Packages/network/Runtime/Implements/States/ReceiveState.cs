using System;
using System.IO;
using Yoozoo.Managers.NetworkV2.DataStruct;

namespace Yoozoo.Managers.NetworkV2.States
{
    public partial class NetClient
    {
        public class ReceiveState : IDisposable
        {
            private const int DefaultBufferLength = 1024 * 64;
            private MemoryStream m_Stream;
            private IPacketHeader m_PacketHeader;
            private bool m_Disposed;

            public ReceiveState()
            {
                m_Stream = new MemoryStream(DefaultBufferLength);
                m_PacketHeader = default(IPacketHeader);
                m_Disposed = false;
            }

            public MemoryStream Stream
            {
                get { return m_Stream; }
            }

            public IPacketHeader PacketHeader
            {
                get { return m_PacketHeader; }
            }

            public void PrepareForPacketHeader(int packetHeaderLength)
            {
                Reset(packetHeaderLength, null);
            }

            public void PrepareForPacket(IPacketHeader packetHeader)
            {
                if (packetHeader == null)
                {
                    throw new Exception("Packet header is invalid.");
                }

                Reset(packetHeader.PacketLength, packetHeader);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (m_Disposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (m_Stream != null)
                    {
                        m_Stream.Dispose();
                        m_Stream = null;
                    }
                }

                m_Disposed = true;
            }

            private void Reset(int targetLength, IPacketHeader packetHeader)
            {
                if (targetLength < 0)
                {
                    throw new Exception("Target length is invalid.");
                }

                m_Stream.Position = 0L;
                m_Stream.SetLength(targetLength);
                m_PacketHeader = packetHeader;
            }
        }
    }
}