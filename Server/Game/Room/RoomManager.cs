using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
        private Room[] _rooms = new Room[4000];
        private object _lock = new object();
        private int _index;

        public void EnterRoom(Player player)
        {
            lock (_lock)
            {
                //TODO: GameState 보안체크 only login
                player.GameState = GameState.Room;
                _players.Add(player.ObjectId, player);

                S_RoomList roomList = new S_RoomList();
                for (int i = 0; i < _rooms.Length; i++)
                {
                    if (_rooms[i] == null)
                        break;

                    roomList.Rooms.Add(_rooms[i].Info);
                }

                player.Session.Send(roomList);
            }
        }

        public void HandleMakeRoom(C_MakeRoom roomInfoPacket, Player player)
        {
            lock (_lock)
            {
                Room room = new Room();
                {
                    room.Idx = _index++;
                    room.State = RoomState.None;
                    room.Title = roomInfoPacket.Title;
                    room.AddPlayer(player);
                    room.OwnerId = roomInfoPacket.Id;
                }

                _rooms[room.Idx] = room;

                S_AddRoom makeRoomPacket = new S_AddRoom {Room = new RoomInfo()};
                makeRoomPacket.Room.MergeFrom(room.Info);

                //'나' 로비로 보내기,'나' 지우기
                _players.Remove(player.ObjectId);
                S_MakeRoomOk makeOk = new S_MakeRoomOk();
                makeOk.Success = true;
                player.Session.Send(makeOk);

                foreach (Player p in _players.Values)
                    p.Session.Send(makeRoomPacket);
            }
        }
    }
}