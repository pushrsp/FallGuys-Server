using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Room
    {
        public RoomInfo Info { get; } = new RoomInfo();

        public RoomState State
        {
            get { return Info.State; }
            set { Info.State = value; }
        }

        public string Title
        {
            get { return Info.Title; }
            set { Info.Title = value; }
        }

        public int Idx
        {
            get { return Info.Idx; }
            set { Info.Idx = value; }
        }

        public string OwnerId
        {
            get { return Info.OwnerId; }
            set { Info.OwnerId = value; }
        }

        public int PlayerCount
        {
            get { return Info.PlayerCount; }
            set { Info.PlayerCount = value; }
        }

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
        private object _lock = new object();

        public void AddPlayer(Player player)
        {
            lock (_lock)
            {
                _players.Add(player.Id, player);
                PlayerCount = _players.Count;
            }
        }
    }
}