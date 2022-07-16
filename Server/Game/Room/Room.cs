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
        private Random _random = new Random();

        public void HandleSpawn()
        {
            S_Spawn spawnPacket = new S_Spawn();
            foreach (Player player in _players.Values)
            {
                player.GameState = GameState.Room;
                player.ResetInfo();
                player.PosInfo.PosX = _random.Next(0, 151);
                spawnPacket.Players.Add(player.Info);
            }
        }

        public void HandleEnterRoom(Player player)
        {
            lock (_lock)
            {
                {
                    player.EnteredRoom = this;
                    player.GameState = GameState.Room;
                    _players.TryAdd(player.ObjectId, player);
                    PlayerCount = _players.Count;
                    player.ResetInfo();
                    player.PosInfo.PosX = _random.Next(0, 151);
                }

                RoomManager.Instance.RemovePlayer(player.ObjectId);

                //방 정보 변경 전송
                {
                    S_ChangeRoom changeRoomPacket = new S_ChangeRoom {Room = new RoomInfo()};
                    changeRoomPacket.Room.MergeFrom(Info);
                    RoomManager.Instance.HandleChangeRoom(changeRoomPacket);
                }

                //본인 전송
                {
                    S_EnterRoom enterRoomPacket = new S_EnterRoom {Player = new PlayerInfo()};
                    enterRoomPacket.Player.MergeFrom(player.Info);
                    enterRoomPacket.CanMove = true;

                    player.Session.Send(enterRoomPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (p.ObjectId == player.ObjectId)
                            continue;

                        spawnPacket.Players.Add(p.Info);
                    }

                    player.Session.Send(spawnPacket);
                }

                //타인 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(player.Info);

                    foreach (Player p in _players.Values)
                    {
                        if (p.ObjectId == player.ObjectId)
                            continue;

                        p.Session.Send(spawnPacket);
                    }
                }
            }
        }

        public void HandleChangePlayer(Player player, C_ChangePlayer changePlayerPacket)
        {
            player.PlayerSelect = changePlayerPacket.PlayerSelect;

            lock (_lock)
            {
                // 디스폰
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.ObjectId.Add(player.ObjectId);

                    Broadcast(despawnPacket);
                }

                //리스폰
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(player.Info);

                    Broadcast(spawnPacket);
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

        public void HandleStartGame(Player player, int stageId)
        {
            if (player.ObjectId != OwnerId)
                return;

            lock (_lock)
            {
                State = RoomState.Playing;
                GameRoom gameRoom = GameManager.Instance.Add(stageId);
                gameRoom.PlayerCount = _players.Count;
                gameRoom.Idx = Idx;
                S_StartGame startGamePacket = new S_StartGame();
                startGamePacket.StageId = stageId;

                foreach (Player p in _players.Values)
                {
                    p.GameRoom = gameRoom;
                    p.GameState = GameState.Game;
                    p.Session.Send(startGamePacket);
                }
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