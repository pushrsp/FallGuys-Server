using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using Server.Game.Object;

namespace Server
{
    public class GameRoom : IRoom
    {
        public int RoomId { get; set; }
        public Stage Stage { get; } = new Stage();

        private object _lock = new object();
        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
        private Dictionary<int, Obstacle> _obstacles = new Dictionary<int, Obstacle>();
        private int _obstacleId = 1;
        private Random _random = new Random();

        public void Init(int stageId)
        {
            Stage.LoadStage(stageId);
        }

        public void Add<T>(float speed, Vector3 pivot) where T : Obstacle, new()
        {
            T obs = new T();
            obs.Room = this;
            obs.Speed = speed;
            obs.Id = _obstacleId++;
            obs.RotateDir = (Obstacle.Dir) _random.Next(0, 2);
            obs.Type = ObstacleType.Rotate;

            if (typeof(T).ToString().Contains("PendulumObs"))
            {
                (obs as PendulumObs).Pivot = pivot;
                obs.Type = ObstacleType.Pendulum;
            }

            _obstacles.Add(obs.Id, obs);
        }

        public void Update()
        {
            foreach (Obstacle obs in _obstacles.Values)
                obs.Update();
        }

        public void HandleEnterRoom(Player player)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                player.GameRoom = this;
                player.GameState = GameState.Game;
                _players.Add(player.ObjectId, player);

                Pos pos = Stage.FindStartPos();
                {
                    player.PosInfo.PosY = pos.Y;
                    player.PosInfo.PosZ = pos.Z;
                    player.PosInfo.PosX = pos.X;
                }

                //Map
                Stage.ApplyMove(player);

                //본인 전송
                {
                    S_EnterRoom enterPacket = new S_EnterRoom
                        {Player = new PlayerInfo {PosInfo = new PositionInfo()}};
                    enterPacket.Player = player.Info;
                    player.Session.Send(enterPacket);

                    //TODO: 장애물 플레이어 구분하기
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (p.ObjectId != player.ObjectId)
                            spawnPacket.Players.Add(p.Info);
                    }

                    S_SpawnObstacle spawnObstacle = new S_SpawnObstacle();
                    foreach (Obstacle obs in _obstacles.Values)
                        spawnObstacle.Obstacles.Add(new ObstacleInfo {ObstacleId = obs.Id, Type = obs.Type});

                    player.Session.Send(spawnPacket);
                    player.Session.Send(spawnObstacle);
                }

                //타인 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(player.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (p.ObjectId != player.ObjectId)
                            p.Session.Send(spawnPacket);
                    }
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

                char valid = Stage.CanGo(dest);
                if (valid == '5')
                {
                    HandleDie(player);
                    return;
                }

                Stage.ApplyMove(player, dest);
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

        public void HandleDie(Player player)
        {
            S_Die diePacket = new S_Die();
            diePacket.ObjectId = player.ObjectId;

            Broadcast(diePacket);
            HandleRespawn(player.PosInfo, player);
        }

        private void HandleRespawn(PositionInfo dest, Player player)
        {
            //본인 전송
            {
                S_EnterRoom enterPacket = new S_EnterRoom
                {
                    Player = new PlayerInfo
                    {
                        PosInfo = new PositionInfo(),
                        MoveDir = new PositionInfo()
                    }
                };

                Tuple<int, int, int> respawnPos = Stage.FindRespawn(dest, player);
                player.PosInfo = new PositionInfo
                {
                    PosY = respawnPos.Item1,
                    PosZ = respawnPos.Item2,
                    PosX = respawnPos.Item3
                };

                enterPacket.Player = player.Info;
                player.Session.Send(enterPacket);
            }

            //타인 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Players.Add(player.Info);

                foreach (Player p in _players.Values)
                {
                    if (p.ObjectId != player.ObjectId)
                        p.Session.Send(spawnPacket);
                }
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

        public void LeaveGame(string objectId)
        {
            lock (_lock)
            {
                Player player;
                if (_players.TryGetValue(objectId, out player) == false)
                    return;

                Stage.ApplyLeave(player.PosInfo);
                player.GameRoom = null;

                //본인 전송
                {
                    S_LeaveGame leaveGame = new S_LeaveGame();
                    player.Session.Send(leaveGame);
                }

                //타인 전송
                {
                    S_Despawn despawn = new S_Despawn();
                    despawn.ObjectId.Add(player.ObjectId);

                    foreach (Player p in _players.Values)
                    {
                        if (p.ObjectId != objectId)
                            p.Session.Send(despawn);
                    }
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