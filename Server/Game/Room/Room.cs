using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Room
    {
        public RoomState State { get; set; } = RoomState.None;
        public string Name { get; set; }
        public int Index { get; set; }
        public Dictionary<string, Player> Players { get; set; } = new Dictionary<string, Player>();
    }
}