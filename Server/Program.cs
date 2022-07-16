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
        public static Dictionary<int, Timer> Timers = new Dictionary<int, Timer>();

        public static void TickRoom(GameRoom room, int tick = 100)
        {
            Timer timer = new Timer();
            timer.Interval = tick;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += (s, e) => { room.Update(); };

            Timers.Add(room.RoomId, timer);
        }

        public static void ClearTimer(int roomId)
        {
            Timer timer;
            if (Timers.TryGetValue(roomId, out timer) == false)
                return;

            timer.Stop();
            timer.Close();
            timer.Dispose();
            Timers.Remove(roomId);
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