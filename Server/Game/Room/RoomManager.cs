using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
        private List<Room> _rooms = new List<Room>();
        private object _lock = new object();
        private int _index = 0;

        public Player EnterRoom(string username, string id, ClientSession session)
        {
            lock (_lock)
            {
                Player player = new Player();

                {
                    player.Id = id;
                    player.Username = username;
                    player.Session = session;
                    _players.Add(player.ObjectId, player);
                }

                Room room = new Room();
                {
                    room.State = RoomState.None;
                    room.Title = "아무나 들어와!";
                    room.Idx = _index++;
                    room.OwnerId = player.Id;
                    room.AddPlayer(player);
                    // room.Players.Add(player.Id, player);
                }
                _rooms.Add(room);

                S_RoomList roomList = new S_RoomList();
                foreach (Room r in _rooms)
                    roomList.Rooms.Add(r.Info);

                player.Session.Send(roomList);

                return player;
            }
        }
    }
}