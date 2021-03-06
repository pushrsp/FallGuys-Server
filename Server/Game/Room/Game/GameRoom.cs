using System;
using System.Collections.Generic;
using System.Timers;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using Server.Game.Object;

namespace Server
{
    public class GameRoom : IRoom
    {
        public int RoomId { get; set; }
        public int Idx { get; set; }
        public int PlayerCount { get; set; }
        public Stage Stage { get; } = new Stage();

        private object _lock = new object();
        private Dictionary<string, Player> _players = new Dictionary<string, Player>();
        private Dictionary<string, Player> _arrivedPlayers = new Dictionary<string, Player>();
        private Dictionary<int, Obstacle> _obstacles = new Dictionary<int, Obstacle>();
        private int _obstacleId = 1;
        private Random _random = new Random();
        private Timer _timerStart = new Timer();
        private Timer _timerEnd = new Timer();

        public void Init(int stageId)
        {
            Stage.LoadStage(stageId);
        }

        public void Clear()
        {
            Program.ClearTimer(RoomId);

            _players.Clear();
            _arrivedPlayers.Clear();
            _obstacles.Clear();

            GameManager.Instance.Remove(RoomId);
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

        private int _counter = 4;

        private void TimerStartTick(object o, ElapsedEventArgs e)
        {
            if (_counter >= 0)
            {
                S_StartCountDown startCountDownPacket = new S_StartCountDown();
                startCountDownPacket.Counter = _counter;

                Broadcast(startCountDownPacket);

                if (_counter == 0)
                {
                    _timerStart.Stop();
                    _timerStart.Close();
                    _timerStart.Dispose();
                }
                else if (_counter == 1)
                {
                    Program.TickRoom(this, 100);
                }
            }

            _counter--;
        }

        private void StartCount()
        {
            _timerStart.Interval = 1000;
            _timerStart.Elapsed += TimerStartTick;
            _timerStart.Start();
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

                if (_players.Count == PlayerCount)
                    StartCount();

                Pos pos = Stage.FindStartPos();
                {
                    player.PosInfo.PosY = pos.Y;
                    player.PosInfo.PosZ = pos.Z;
                    player.PosInfo.PosX = pos.X;
                }

                //Map
                Stage.ApplyMove(player);

                //?????? ??????
                {
                    S_EnterRoom enterPacket = new S_EnterRoom
                        {Player = new PlayerInfo {PosInfo = new PositionInfo()}};
                    enterPacket.Player.MergeFrom(player.Info);
                    enterPacket.CanMove = false;
                    player.Session.Send(enterPacket);

                    //TODO: ????????? ???????????? ????????????
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

                //?????? ??????
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

        private bool _arrived;
        private int _endCounter = 11;

        private void TimerEndTick(object o, ElapsedEventArgs e)
        {
            if (_endCounter >= 0)
            {
                S_EndCountDown endCountDownPacket = new S_EndCountDown();
                endCountDownPacket.Counter = _endCounter;

                Broadcast(endCountDownPacket);

                //TODO: ????????? ????????????
                if (_counter == 0)
                {
                    Clear();
                    _timerEnd.Stop();
                    _timerEnd.Close();
                    _timerEnd.Dispose();
                }
            }

            _endCounter--;
        }

        private void EndCount()
        {
            if (_arrived)
                return;

            _arrived = true;
            _timerEnd.Interval = 1000;
            _timerEnd.Elapsed += TimerEndTick;
            _timerEnd.Start();
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
                //??????
                else if (valid == '3')
                {
                    if (_arrivedPlayers.TryAdd(player.ObjectId, player))
                    {
                        S_Arrive arrivePacket = new S_Arrive();
                        arrivePacket.ObjectId = player.ObjectId;
                        player.State = PlayerState.Idle;
                        Broadcast(arrivePacket);
                    }

                    EndCount();
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
            //?????? ??????
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

                enterPacket.Player.MergeFrom(player.Info);
                enterPacket.CanMove = true;
                player.Session.Send(enterPacket);
            }

            //?????? ??????
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

                //?????? ??????
                {
                    S_LeaveGame leaveGame = new S_LeaveGame();
                    player.Session.Send(leaveGame);
                }

                //?????? ??????
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