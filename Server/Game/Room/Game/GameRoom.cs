using System;
using System.Collections.Generic;
using System.Timers;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using Server.Game.Object;

namespace Server
{
    public class GameRoom : BaseRoom
    {
        public int RoomId { get; set; }
        public int Idx { get; set; }
        public int PlayerCount { get; set; }
        public Stage Stage { get; } = new Stage();

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

        private void Clear()
        {
            Program.ClearTimer(RoomId);

            _players.Clear();
            _arrivedPlayers.Clear();
            _obstacles.Clear();

            GameManager.Instance.Remove(RoomId);
        }

        //장애물 추가
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

        public override void HandleEnterRoom(Player player)
        {
            if (player == null)
                return;

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

            //본인 전송
            {
                S_EnterRoom enterPacket = new S_EnterRoom
                    {Player = new PlayerInfo {PosInfo = new PositionInfo()}};
                enterPacket.Player.MergeFrom(player.Info);
                enterPacket.CanMove = false;
                player.Session.Send(enterPacket);

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

        private bool _arrived;
        private int _endCounter = 11;

        private void TimerEndTick(object o, ElapsedEventArgs e)
        {
            if (_endCounter >= 0)
            {
                S_EndCountDown endCountDownPacket = new S_EndCountDown();
                endCountDownPacket.Counter = _endCounter;

                Broadcast(endCountDownPacket);

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

        public override void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            PositionInfo dest = movePacket.PosInfo;
            PositionInfo dir = movePacket.MoveDir;

            char valid = Stage.CanGo(dest);
            if (valid == '5')
            {
                Push<Player>(HandleDie, player);
                return;
            }

            if (valid == '3') //도착
            {
                if (_arrivedPlayers.TryAdd(player.ObjectId, player))
                {
                    S_Arrive arrivePacket = new S_Arrive();
                    arrivePacket.ObjectId = player.ObjectId;
                    player.State = PlayerState.Idle;
                    Push<IMessage>(Broadcast, arrivePacket);
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

            Push<IMessage>(Broadcast, resMovePacket);
        }

        public void HandleDie(Player player)
        {
            S_Die diePacket = new S_Die();
            diePacket.ObjectId = player.ObjectId;

            Push<IMessage>(Broadcast, diePacket);
            Push<PositionInfo, Player>(HandleRespawn, player.PosInfo, player);
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

                enterPacket.Player.MergeFrom(player.Info);
                enterPacket.CanMove = true;
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

        public override void HandleJump(Player player)
        {
            if (player == null)
                return;

            S_Jump resJumpPacket = new S_Jump();
            resJumpPacket.ObjectId = player.ObjectId;

            Push<IMessage>(Broadcast, resJumpPacket);
        }

        public void LeaveGame(string objectId)
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

        public override void Broadcast(IMessage packet)
        {
            foreach (Player p in _players.Values)
                p.Session.Send(packet);
        }
    }
}