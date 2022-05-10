using System.Net.Sockets;
using System.Text;

namespace NUnit.Agent
{
    public class SocketClient// : IIpcClient
    {
        //        public const int PORT = 9004;
        int m_port = 9004;
        public SocketClient(int port)
        {
            m_port = port;
        }

        public void Send(string data)
        {
            using (var client = new UdpClient())
            {
                client.Connect(string.Empty, m_port);
                var bytes = Encoding.Default.GetBytes(data);
                client.Send(bytes, bytes.Length);
            }
        }
    }
}
