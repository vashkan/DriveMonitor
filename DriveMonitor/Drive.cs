using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Socket = System.Net.Sockets.Socket;
using System.Threading;

namespace DriveMonitor
{
    class Drive
    {
        public byte[] h_sendPower=new byte[4] { 0, 0, 0, 0 };
        public byte[] h_recvPower = new byte[4] { 0, 0, 0, 0 };

        public static byte MaxPower = 100, MinPower = 0;

        public object mre=new object();
        
        public byte[] SendPower
        {
            get
            {
                return h_sendPower;
            }
            set
            {
                h_sendPower = value;
            }
        }

        public byte[] RecvPower
        {
            get
            {
                return h_recvPower;
            }
            set
            {
                h_recvPower = value;
            }
        }
        
        public void IncPower()
        {
            lock (mre)
            {
                for (int pow = 0; pow < SendPower.Length; pow++)
                    for (int j = 0; j < 3; j++)
                        if (SendPower[pow] + 1 <= MaxPower)
                            SendPower[pow] += 1;
                Monitor.Pulse(mre);
            }
        }

        public void DecPower()
        {
            lock (mre)
            {
                for (int pow = 0; pow < SendPower.Length; pow++)
                    for (int j = 0; j < 3; j++)
                        if (SendPower[pow] - 1 >= MinPower)
                            SendPower[pow] -= 1;
                Monitor.Pulse(mre);
            }
        }

        public void Forward()
        {
            lock (mre)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (SendPower[2 + i] - SendPower[i] < 20)
                    {
                        if (SendPower[i] - 1 >= MinPower)
                            SendPower[i] -= 1;

                        if (SendPower[2 + i] + 1 <= MaxPower)
                            SendPower[2 + i] += 1;
                    }
                }
                Monitor.Pulse(mre);
            }
        }

        public void Back()
        {
            lock (mre)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (SendPower[i] - SendPower[2 + i] < 20)
                    {
                        if (SendPower[2 + i] - 1 >= MinPower)
                            SendPower[2 + i] -= 1;

                        if (SendPower[i] + 1 <= MaxPower)
                            SendPower[i] += 1;
                    }
                }
                Monitor.Pulse(mre);
            }
        }

        public void Left()
        {
            lock (mre)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (SendPower[2 * i + 1] - SendPower[2 * i] < 20)
                    {
                        if (SendPower[2 * i] - 1 >= MinPower)
                            SendPower[2 * i] -= 1;
                        if (SendPower[2 * i + 1] + 1 <= MaxPower)
                            SendPower[2 * i + 1] += 1;

                    }
                }
                Monitor.Pulse(mre);
            }
        }

        public void Right()
        {
            lock (mre)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (SendPower[2 * i] - SendPower[2 * i + 1] < 20)
                    {
                        if (SendPower[2 * i + 1] - 1 >= MinPower)
                            SendPower[2 * i + 1] -= 1;

                        if (SendPower[2 * i] + 1 <= MaxPower)
                            SendPower[2 * i] += 1;
                    }
                }
                Monitor.Pulse(mre);
            }
        }

        public void Smooth()
        {
            lock (mre)
            {
                int i, s;
                s = Convert.ToInt32((SendPower[0] + SendPower[1] + SendPower[2] + SendPower[3]) / 4);
                for (i = 0; i <= 3; i++)
                {
                    if ((SendPower[i] <= MaxPower) && (SendPower[i] >= MinPower))
                    {
                        if (SendPower[i] < s) SendPower[i]++;
                        if (SendPower[i] > s) SendPower[i]--;
                    }
                }
                Monitor.Pulse(mre);
            }
        }
    }

    class Client
    {
        Socket serverSocket;
        String host;
        Int32 port;
        public string Status;
        public Drive Motors;
        public bool m_formClosed;

        public Client()
        {
            host = "192.168.0.103";
            port = 12000;
        }

        public Client(Drive Motors):this()
        {
            this.Motors = Motors;
            new Thread(new ThreadStart(Start)).Start();
        }

        void Start()
        {
            bool _firstRun = true;

            using (serverSocket = ConnectSocket(host, port))
            {
                Status = "conected";
                m_formClosed = false;
                while (serverSocket.Connected && !m_formClosed)
                {
                    lock (Motors.mre)
                    {
                        if (_firstRun)
                        {
                            _firstRun = false;
                        }
                        else
                        {
                            Monitor.Wait(Motors.mre);
                        }

                        Byte[] powerToSend = Motors.SendPower;

                        serverSocket.Send(powerToSend, powerToSend.Length, 0);
                    }
                    Status = "send";

                    Byte[] buffer = new Byte[4];

                    Int32 bytesRead = serverSocket.Receive(buffer);

                    Status = "check";
                    Motors.RecvPower = buffer;
                }
            }
        }

        private Socket ConnectSocket(String server, Int32 port)
        {
            // Get server's IP address.

            // Create socket and connect to the server's IP address and port
            Socket socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Parse(server), port));
            return socket;
        }
    }
}

