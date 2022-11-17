using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class ClientGame
    {
        //game objects
        private Ball ball;
        private Paddle leftPaddle;
        private Paddle rightPaddle;
        private float prevY; //prev postion of paddle
        //!game objects

        //net basics
        public readonly string host;
        public readonly int port;
        private UdpClient udpClient;
        //!net basics

        //messages
        private Thread thread;
        private ConcurrentQueue<Message> inMessages = new ConcurrentQueue<Message>();
        private ConcurrentQueue<Packet> outMessages = new ConcurrentQueue<Packet>();
        //!messages

        //states
        private ClientState state = ClientState.NotConnected;
        private Locked<bool> running = new Locked<bool>();
        private Locked<bool> send_end_game_pack = new Locked<bool>();
        //!states

        public void Start()
        {
            running.var = true;
            state = ClientState.Connecting;
            thread = new Thread(() => netRun());
            thread.Start();
        }

        private void netRun()
        {
            while (running.var)
            {
                bool canRead = udpClient.Available > 0;
                int numToWrite = outMessages.Count;

                // Get data if there is some
                if (canRead)
                {
                    // Read in one datagram
                    IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udpClient.Receive(ref ep);              // Blocks

                    // Enque a new message
                    Message nm = new Message
                    {
                        sender = ep,
                        packet = new Packet(data),
                        recvTime = DateTime.Now
                    };

                    inMessages.Enqueue(nm);

                    Console.WriteLine("RCVD: {0}", nm.packet);
                }

                // Write out queued
                for (int i = 0; i < numToWrite; i++)
                {
                    // Send some data
                    Packet packet;
                    bool have = outMessages.TryDequeue(out packet);
                    if (have)
                        packet.Send(udpClient);

                    Console.WriteLine("SENT: {0}", packet);
                }

                // If Nothing happened, take a nap
                if (!canRead && (numToWrite == 0))
                    Thread.Sleep(1);
            }
        }

        public ClientGame(string host, int port)
        {
            this.host = host;
            this.port = port;
            this.udpClient = new UdpClient(host, port);

            ball = new Ball();
            leftPaddle = new Paddle();
            rightPaddle = new Paddle();

        }
    }
}
