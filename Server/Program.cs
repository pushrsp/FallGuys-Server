using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Timers;
using Core;
using Timer = System.Timers.Timer;

namespace Server
{
    public class Program
    {
        private static Listener _listener = new Listener();
        private static List<Timer> _timers = new List<Timer>();

        public static void TickRoom(GameRoom room, int tick = 100)
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

            _listener.Init(endPoint, () => SessionManager.Instance.Generate());
            Console.WriteLine("Listening...");

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}