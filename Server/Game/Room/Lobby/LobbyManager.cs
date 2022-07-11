using System.Collections.Generic;

namespace Server.Game
{
    public class LobbyManager
    {
        public LobbyManager Instance { get; } = new LobbyManager();

        private object _lock = new object();
        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
    }
}