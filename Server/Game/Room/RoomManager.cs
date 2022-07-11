using System.Collections.Generic;

namespace Server.Game
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
        private List<Room> _rooms = new List<Room>();
        private object _lock = new object();

        public Player EnterRoom(string username, string id)
        {
            lock (_lock)
            {
                Player player = new Player();

                {
                    player.Id = id;
                    player.Username = username;
                    _players.Add(player.ObjectId, player);
                }

                //TODO: room broadcast

                return player;
            }
        }
    }
}