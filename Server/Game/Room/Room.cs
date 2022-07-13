using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Room : IRoom
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

        public void HandleEnterRoom(Player player)
        {
            lock (_lock)
            {
                S_SpawnInRoom spawnRoomPacket = new S_SpawnInRoom {Me = new PlayerInfo()};

                {
                    player.Room = this;
                    player.GameState = GameState.Lobby;
                    _players.Add(player.ObjectId, player);
                    PlayerCount = _players.Count;
                    player.ResetInfo();
                }

                RoomManager.Instance.RemovePlayer(player.ObjectId);

                //방 정보 변경 전송
                {
                    S_ChangeRoom changeRoomPacket = new S_ChangeRoom {Room = new RoomInfo()};
                    changeRoomPacket.Room.MergeFrom(Info);
                    RoomManager.Instance.HandleChangeRoom(changeRoomPacket);
                }

                spawnRoomPacket.Me.MergeFrom(player.Info);

                foreach (Player otherPlayers in _players.Values)
                {
                    if (otherPlayers.ObjectId == player.ObjectId)
                        continue;

                    spawnRoomPacket.Players.Add(otherPlayers.Info);
                }

                ClientSession clientSession = player.Session;
                if (clientSession == null)
                    return;

                //본인 전송
                clientSession.Send(spawnRoomPacket);

                //타인전송
                foreach (Player p in _players.Values)
                {
                    if (p.ObjectId == player.ObjectId)
                        continue;

                    S_SpawnInRoom spawnInRoom = new S_SpawnInRoom();
                    spawnInRoom.Players.Add(player.Info);
                    p.Session.Send(spawnInRoom);
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                PositionInfo dest = movePacket.PosInfo;
                PositionInfo dir = movePacket.MoveDir;

                player.PosInfo = dest;
                player.MoveDir = dir;

                S_Move resMovePacket = new S_Move {MoveDir = new PositionInfo(), DestPos = new PositionInfo()};
                resMovePacket.ObjectId = player.ObjectId;
                resMovePacket.MoveDir = dir;
                resMovePacket.DestPos = dest;
                resMovePacket.State = movePacket.State;

                Broadcast(resMovePacket);
            }
        }

        public void HandleJump(Player player)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                S_Jump resJumpPacket = new S_Jump();
                resJumpPacket.ObjectId = player.ObjectId;

                Broadcast(resJumpPacket);
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                    p.Session.Send(packet);
            }
        }
    }
}