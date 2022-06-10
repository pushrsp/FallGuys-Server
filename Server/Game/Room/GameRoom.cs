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
                S_EnterGame enterPacket = new S_EnterGame {PlayerInfo = new PlayerInfo {PosInfo = new PositionInfo()}};
                enterPacket.PlayerInfo = player.Info;
                player.Session.Send(enterPacket);

                //타인 전송
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.PlayerInfo.Add(player.Info);
                foreach (Player p in _players.Values)
                {
                    if (p != player)
                        p.Session.Send(spawnPacket);
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