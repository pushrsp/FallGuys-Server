using System.Collections.Generic;

namespace Server.Game.Object
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        private object _lock = new object();
        private Dictionary<string, Player> _players = new Dictionary<string, Player>();

        public Player Add(string id)
        {
            Player player = new Player();
            lock (_lock)
            {
                player.ObjectId = id;
                _players.Add(player.ObjectId, player);
            }

            return player;
        }
    }
}