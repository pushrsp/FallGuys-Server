using System;
using System.Net;
using Core;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using Server.Game.Object;

// ReSharper disable All

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public int SessionId { get; set; }
        public Player Me { get; set; }

        private Random _random = new Random();

        public void Send(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId) Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort) packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort) size + 4), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort) msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);
            Send(new ArraySegment<byte>(sendBuffer));
        }

        public override void OnConnected(EndPoint endPoint)
        {
            {
                S_Connected connectedOk = new S_Connected();
                Send(connectedOk);
            }
        }

        public override void OnSend(int numOfBytes)
        {
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            // GameRoom room = GameManager.Instance.Find(1);
            // room.LeaveGame(Me.ObjectId);
            //
            // SessionManager.Instance.Remove(this);
            //
            // Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }
    }
}