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
    public abstract class NetConnection
    {
        public delegate void NetConnectionEventHandler<TEventArgs>(object sender, NetConnection connection, TEventArgs e);
        public delegate void NetConnectionEventHandler(object sender, NetConnection connection);

        public event NetConnectionEventHandler OnConnect;
        public event NetConnectionEventHandler OnDisconnect;
        public event NetConnectionEventHandler<byte[]> OnDataSend;
        public event NetConnectionEventHandler<byte[]> OnDataReceived;

        public TcpClient client;

        protected List<Task> tasks = new List<Task>();
        protected List<CancellationTokenSource> cancellations = new List<CancellationTokenSource>();

        protected bool CleanUpPending = false;


        public enum ClientMode
        {
            eventMode, bufferMode
        }

        private ClientMode mode = ClientMode.eventMode;

        protected List<byte> rxBuffer = new List<byte>();
        private System.Timers.Timer rxTimer = new System.Timers.Timer();
        private const int rxTimerFreq = 5;
        private Semaphore rxToken = new Semaphore(initialCount: 1, maximumCount: 1);


        public NetConnection()
        {
            mode = ClientMode.eventMode;
        }

        public NetConnection(ClientMode clientMode)
        {
            mode = clientMode;
            if (mode == ClientMode.bufferMode)
            {
                rxTimer.Elapsed += new ElapsedEventHandler(RxTimerEvent);
                rxTimer.Interval = rxTimerFreq;
            }
            rxTimer.Enabled = false;
        }

        #region "Data Receive"

        private const int BufferSize = 1024;

        protected void StartReceive(NetConnection connection)
        {
            switch (mode)
            {
                case ClientMode.eventMode:
                    tasks.Add(ReceiveFromAsync(connection));
                    break;
                case ClientMode.bufferMode:
                    rxToken.WaitOne();
                    rxBuffer.Clear();
                    rxToken.Release();
                    rxTimer.Enabled = true;
                    break;
            }
        }

        protected void StopReceive()
        {
            switch (mode)
            {
                case ClientMode.eventMode:
                    break;
                case ClientMode.bufferMode:
                    rxTimer.Enabled = false;
                    break;
            }
        }

        #region "Data Receive - Event Mode"

        protected void StartReceiveFrom(NetConnection connection)
        {
            tasks.Add(ReceiveFromAsync(connection));
        }

        protected async Task ReceiveFromAsync(NetConnection connection)
        {
            try
            {
                NetworkStream stream = connection.client.GetStream();
                byte[] buffer = new byte[BufferSize];
                MemoryStream ms = new MemoryStream();
                CancellationTokenSource cancellation = new CancellationTokenSource();
                cancellations.Add(cancellation);
                while (connection.client.Connected)
                {
                    int bytesRead = await Task.Run(
                        () => stream.ReadAsync(buffer, 0, buffer.Length),
                        cancellation.Token);
                    ms.Write(buffer, 0, bytesRead);
                    if (!stream.DataAvailable)
                    {
                        CallOnDataReceived(connection, ms.ToArray());
                        ms.Seek(0, SeekOrigin.Begin);
                        ms.SetLength(0);
                    }
                }
                CallOnDisconnect(connection);
            }
            catch
            {
                CallOnDisconnect(connection);
                throw;
            }
        }

        #endregion "Data Receive - Event Mode"

        #region "Data Receive - Buffer Mode"


        public int Available
        {
            get { return rxBuffer.Count; }
        }

        public void ClearRxBuffer()
        {
            rxToken.WaitOne();
            rxBuffer.Clear();
            rxToken.Release();

            // try to clearn buffer in stream also
            if ((client != null) && (client.Connected) && (client.Available > 0))
            {
                Stream stm = client.GetStream();
                int nBytes = client.Available;
                byte[] buffer = new byte[nBytes];
                stm.Read(buffer, 0, client.Available);
            }
        }

        public byte Read()
        {
            if (Available == 0) return 0;
            rxToken.WaitOne();
            byte data = rxBuffer[0];
            rxBuffer.RemoveAt(0);
            rxToken.Release();
            return data;
        }

        public byte[] ReadAll()
        {
            rxToken.WaitOne();
            byte[] data = rxBuffer.ToArray();
            rxBuffer.Clear();
            rxToken.Release();
            return data;
        }

        private void RxTimerEvent(object source, ElapsedEventArgs e)
        {
            rxTimer.Enabled = false;
            if ((client != null) && (client.Connected) && (client.Available > 0))
            {
                Stream stm = client.GetStream();
                int nBytes = client.Available;
                byte[] buffer = new byte[nBytes];
                stm.Read(buffer, 0, client.Available);
                rxToken.WaitOne();
                rxBuffer.AddRange(buffer);
                rxToken.Release();
            }
            rxTimer.Enabled = true;
        }
        #endregion "Data Receive - Buffer Mode"

        #endregion "Data Receive"


        public EndPoint RemoteEndPoint
        {
            get
            {
                if (client == null) return null;
                return client.Client.RemoteEndPoint;
            }
        }

        protected void CallOnDataReceived(NetConnection connection, byte[] data)
        {
            OnDataReceived?.Invoke(this, connection, data);
        }
        protected void CallOnDataSend(NetConnection connection, byte[] data)
        {
            OnDataSend?.Invoke(this, connection, data);
        }
        protected void CallOnConnect(NetConnection client)
        {
            OnConnect?.Invoke(this, client);
        }
        protected void CallOnDisconnect(NetConnection client)
        {
            OnDisconnect?.Invoke(this, client);
        }

        public virtual bool Send(string data)
        {
            if (client == null) return false;
            return Send(Encoding.UTF8.GetBytes(data));
        }

        public virtual bool Send(byte[] data)
        {
            if (client == null) return false;
            CallOnDataSend(this, data);
            client.GetStream().Write(data, 0, data.Length);
            return true;
        }

        private class CT
        {
            public Task t;
            public TaskStatus s;

            public CT(Task t, TaskStatus s)
            {
                this.t = t;
                this.s = s;
            }
        };

        protected void CleanUp()
        {
            // No need to wait for task with status:
            // 1 - Faulted : already disconnected
            // 2 - RanToCompletion : already completed
            // 3 - Canceled : already canceled
            foreach (Task t in tasks)
            {
                if ((t.Status != TaskStatus.Faulted) && 
                    (t.Status != TaskStatus.RanToCompletion) &&
                    (t.Status != TaskStatus.Canceled))
                {
                    t.Wait();
                }
            }
            foreach (Task t in tasks)
            {
                t.Dispose();
            }
            foreach (CancellationTokenSource c in cancellations)
            {
                c.Dispose();
            }
            tasks = new List<Task>();
            cancellations = new List<CancellationTokenSource>();
            CleanUpPending = false;
        }

    }
}