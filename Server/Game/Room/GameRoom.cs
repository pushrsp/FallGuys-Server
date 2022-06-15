using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using Server.Game.Object;

namespace Server
{
    public class GameRoom
    {
        public int RoomId { get; set; }
        public Stage Stage { get; } = new Stage();

        private object _lock = new object();
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private List<RotateObs> _rotateObs = new List<RotateObs>();
        private List<WheelObs> _wheelObs = new List<WheelObs>();

        public void Init(int stageId)
        {
            Stage.LoadStage(stageId);
        }

        public void AddRotateObs()
        {
            RotateObs obs = new RotateObs();
            obs.Room = this;
            obs.Speed = 40.0f;

            _rotateObs.Add(obs);
        }

        public void AddWheelObs()
        {
            WheelObs obs = new WheelObs();
            obs.Room = this;
            obs.Speed = 10.0f;

            _wheelObs.Add(obs);
        }

        public void Update()
        {
            // if (_players.Count == 0)
            //     return;
            //
            // foreach (RotateObs rotateObs in _rotateObs)
            //     rotateObs.Update();
        }

        public void EnterRoom(Player player)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                player.Room = this;
                _players.Add(player.ObjectId, player);

                //Map
                Stage.ApplyMove(player);

                //본인 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame
                        {PlayerInfo = new PlayerInfo {PosInfo = new PositionInfo()}};
                    enterPacket.PlayerInfo = player.Info;
                    player.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if (p.ObjectId != player.ObjectId)
                            spawnPacket.PlayerInfo.Add(p.Info);
                    }

                    player.Session.Send(spawnPacket);
                }

                //타인 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.PlayerInfo.Add(player.Info);
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

        public void LeaveGame(int objectId)
        {
            lock (_lock)
            {
                Player player;
                if (_players.TryGetValue(objectId, out player) == false)
                    return;

                Stage.ApplyLeave(player.PosInfo);
                player.Room = null;

                //본인 전송
                {
                    S_LeaveGame leaveGame = new S_LeaveGame();
                    player.Session.Send(leaveGame);
                }

                //타인 전송
                {
                    S_Despawn despawn = new S_Despawn();
                    despawn.PlayerId.Add(player.ObjectId);

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