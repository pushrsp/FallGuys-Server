using System.Collections.Generic;

namespace Server.Game.Object
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        private object _lock = new object();
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private int objectId = 1;

        public Player Add()
        {
            Player player = new Player();
            lock (_lock)
            {
                player.ObjectId = objectId++;
                _players.Add(player.ObjectId, player);
            }

            return player;
        }
    }
}