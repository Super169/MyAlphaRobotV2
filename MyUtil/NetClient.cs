using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MyUtil
{
    public class NetClient : NetConnection
    {
        public bool Connected
        {
            get
            {
                if (client == null) return false;
                return client.Connected;
            }
        }

        public NetClient()
        {
        }

        public NetClient(ClientMode clientMode) : base(clientMode)
        {
        }

        public NetClient(TcpClient client)
        {
            this.client = client;
        }
        
        public void Connect(string hostname, int port)
        {
            if (client == null)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(hostname, port);

                    CallOnConnect(this);
                    StartReceive(this);
                }
                catch
                {
                    if (client != null)
                    {
                        client.Dispose();
                        client = null;
                    }
                }
            }
        }

        public void Connect(IPAddress address, int port)
        {
            if (client == null)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(address, port);

                    CallOnConnect(this);
                    StartReceive(this);
                }
                catch
                {
                    if (client != null)
                    {
                        client.Dispose();
                        client = null;
                    }
                }
            }

        }

        public void Disconnect()
        {
            CallOnDisconnect(this);
            StopReceive();
            client.LingerState = new LingerOption(true, 0);
            // Don't call GetStream().Close(), otherwise, it will wait for few minutes before close
            // tcpClient.GetStream().Close();
            client.Client.Close();
            client.Close();
            // client.Dispose();
            client = null;
        }


    }
}
