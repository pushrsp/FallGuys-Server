using System.Collections.Generic;

namespace Server
{
    public class GameManager
    {
        public static GameManager Instance { get; } = new GameManager();

        private object _lock = new object();
        private Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        private int _roomId = 1;

        public GameRoom Add(int mapId)
        {
            GameRoom room = new GameRoom();

            lock (_lock)
            {
                room.RoomId = _roomId++;
                _rooms.Add(room.RoomId, room);
            }

            room.Init(mapId);

            return room;
        }

        public void Remove(int roomId)
        {
            _rooms.Remove(roomId);
        }

        public GameRoom Find(int roomId)
        {
            lock (_lock)
            {
                GameRoom room;
                if (_rooms.TryGetValue(roomId, out room) == false)
                    return null;

                return room;
            }
        }
    }
}