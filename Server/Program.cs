using System;
using System.Net;
using System.Threading.Channels;
using Core;

namespace Server
{
    class Program
    {
        private static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7777);

            _listener.Init(endPoint, () => SessionManager.Instance.Generate());
            Console.WriteLine("Listening...");

            while (true)
            {
            }
        }
    }
}