using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;

namespace Server
{
    public class GameRoom
    {
        public int RoomId { get; set; }
        public Stage Stage { get; } = new Stage();

        private object _lock = new object();
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public void Init(int stageId)
        {
            Stage.LoadStage(stageId);
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

            PositionInfo dest = movePacket.PosInfo;
            PositionInfo dir = movePacket.MoveDir;

            player.PosInfo = dest;
            player.MoveDir = dir;
            player.State = movePacket.State;

            S_Move resMovePacket = new S_Move
            {
                PlayerInfo = new PlayerInfo
                {
                    PosInfo = new PositionInfo(),
                    MoveDir = new PositionInfo()
                }
            };

            PlayerInfo info = resMovePacket.PlayerInfo;
            {
                info.ObjectId = player.ObjectId;
                info.State = player.State;
                info.PosInfo = player.PosInfo;
                info.MoveDir = player.MoveDir;
                info.Speed = player.Speed;
            }

            Broadcast(resMovePacket);
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