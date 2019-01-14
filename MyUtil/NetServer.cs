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
    public class NetServer : NetConnection
    {
        public event NetConnectionEventHandler OnKillClient;
        protected void CallOnKillClient(NetConnection client)
        {
            OnKillClient?.Invoke(this, client);
        }

        private TcpListener listener;
        System.Timers.Timer cleanUpTimer = new System.Timers.Timer();
        List<NetClient> clients = new List<NetClient>();

        public NetServer()
        {
            OnDisconnect += ClientOnDisconnect;
            cleanUpTimer.Elapsed += new ElapsedEventHandler(OnCleanUpTimedEvent);
            // 1 sec is good enough to let all task ended, but this may not be a good way to stop the server
            // need to find a better way to perform cleanup after all task cancelled
            cleanUpTimer.Interval = 1000;
            cleanUpTimer.Enabled = false;
        }

        private NetServer(TcpClient client)
        {
            this.client = client;
        }

        public void Start(int port)
        {
            Start(IPAddress.Any, port);
        }

        public void Start(IPAddress address, int port)
        {
            if (listener == null)
            {
                listener = new TcpListener(address, port);
                listener.Start();
                StartListen();
            }
        }
        public void Stop()
        {
            listener.Stop();
            if (clients.Count > 0)
            {
                foreach (NetClient client in clients)
                {
                    CallOnKillClient(client);
                    client.Disconnect();
                }
                clients.Clear();
            }
            foreach (CancellationTokenSource cancellation in cancellations)
            {
                cancellation.Cancel();
            }

            cleanUpTimer.Enabled = true;

        }

        public override bool Send(String data)
        {
            return Send(Encoding.UTF8.GetBytes(data));
        }

        public override bool Send(byte[] data)
        {
            bool success = true;
            foreach (NetClient client in clients)
            {
                // Make sure it will try sending to all clients
                bool result = client.Send(data);
                success &= result;
            }
            return true;
        }

        private void OnCleanUpTimedEvent(object source, ElapsedEventArgs e)
        {
            cleanUpTimer.Enabled = false;
            if (CleanUpPending) return;
            CleanUpPending = true;
            CleanUp();
            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }
        }

        private void StartListen()
        {
            tasks.Add(ListenAsync());
        }

        private async Task ListenAsync()
        {
            try
            {
                CancellationTokenSource cancellation = new CancellationTokenSource();
                cancellations.Add(cancellation);
                while (true)
                {
                    // TcpClient client = await listener.AcceptTcpClientAsync();
                    TcpClient client = await Task.Run(
                           () => listener.AcceptTcpClientAsync(),
                           cancellation.Token);
                    NetClient netClient = new NetClient(client);
                    netClient.OnDataSend += ClientOnSend;
                    clients.Add(netClient);
                    StartReceiveFrom(netClient);
                    CallOnConnect(netClient);
                }
            }
            catch
            {
                Console.WriteLine("Task cancelled");
            }
        }

        private void ClientOnDisconnect(object sender, NetConnection connection)
        {
            EndPoint ep = connection.RemoteEndPoint;
            NetClient client = clients.Find(x => x.RemoteEndPoint == ep);
            if (client != null) clients.Remove(client);
        }

        private void ClientOnSend(object sender, NetConnection connection, byte[] e)
        {
            CallOnDataSend(connection, e);
        }

        public List<NetClient> GetClientList()
        {
            return clients;
        }

    }
}
