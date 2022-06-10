using System;
using System.Net;
using Core;

namespace Server
{
    class Program
    {
        private static Listener Listener = new Listener();

        static void Main(string[] args)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 7777);

            while (true)
            {
            }
        }
    }
}