using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using Core;
using Timer = System.Timers.Timer;

namespace Server
{
    class Program
    {
        private static Listener _listener = new Listener();
        private static List<Timer> _timers = new List<Timer>();

        static void TickRoom(GameRoom room, int tick = 100)
        {
            Timer timer = new Timer();
            timer.Interval = tick;
            timer.Elapsed += (s, e) => room.Update();
            timer.AutoReset = true;
            timer.Enabled = true;

            _timers.Add(timer);
        }

        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7777);

            TickRoom(RoomManager.Instance.Add(1), 50);

            _listener.Init(endPoint, () => SessionManager.Instance.Generate());
            Console.WriteLine("Listening...");

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}