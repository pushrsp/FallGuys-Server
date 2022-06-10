using System;
using System.Net;
using System.Threading.Channels;
using Core;

namespace Server
{
    class Program
    {
        private static Listener Listener = new Listener();

        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7777);

            Console.WriteLine("HI");
            while (true)
            {
            }
        }
    }
}