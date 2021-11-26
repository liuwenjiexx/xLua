using System;
using System.IO;

namespace Yoozoo.Managers.NetworkV2.States
{
    public partial class NetClient
    {
        public class SendState : IDisposable
        {
            private const int DefaultBufferLength = 1024 * 64;
            private MemoryStream m_Stream;
            private bool m_Disposed;
            private bool m_NeedResend;

            public SendState()
            {
                m_Stream = new MemoryStream(DefaultBufferLength);
                m_Disposed = false;
            }

            public MemoryStream Stream
            {
                get { return m_Stream; }
            }

            public bool NeedResend
            {
                get { return m_NeedResend; }
            }

            public void Reset()
            {
                m_Stream.Position = 0L;
                m_Stream.SetLength(0L);
                m_NeedResend = false;
            }


            public void NeedToResend(bool resend)
            {
                m_NeedResend = resend;
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
        }
    }
}