using System.Collections.Generic;

namespace Server
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

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