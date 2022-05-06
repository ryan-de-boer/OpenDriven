using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenDriven.Commands
{
  [Serializable]
  public sealed class DataReceivedEventArgs : EventArgs
  {
    public DataReceivedEventArgs(string data)
    {
      this.Data = data;
    }

    public string Data { get; private set; }
  }

  public sealed class SocketServer : IDisposable //: IIpcServer
  {
    public const int PORT = 9004;
    private readonly UdpClient server = new UdpClient(PORT);

    void IDisposable.Dispose()
    {
      this.Stop();

      (this.server as IDisposable).Dispose();
    }

    public void Start()
    {
      Task.Factory.StartNew(() =>
      {
        var ip = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
          var bytes = this.server.Receive(ref ip);
          var data = Encoding.Default.GetString(bytes);
          this.OnReceived(new DataReceivedEventArgs(data));
        }
      });
    }

    private void OnReceived(DataReceivedEventArgs e)
    {
      var handler = this.Received;

      if (handler != null)
      {
        handler(this, e);
      }
    }

    public void Stop()
    {
      this.server.Close();
    }

    public event EventHandler<DataReceivedEventArgs> Received;
  }
}
