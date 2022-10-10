using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class BaseRoom : JobDispatcher, IRoom
    {
        protected Dictionary<string, Player> _players = new Dictionary<string, Player>();

        public virtual void HandleEnterRoom(Player player)
        {
        }

        public virtual void HandleMove(Player player, C_Move movePacket)
        {
        }

        public virtual void HandleJump(Player player)
        {
        }

        public virtual void Broadcast(IMessage packet)
        {
            foreach (Player p in _players.Values)
                p.Session.Send(packet);
        }

        public virtual void Broadcast(IMessage packet, string objectId)
        {
            foreach (Player p in _players.Values)
            {
                if (p.ObjectId != objectId)
                    p.Session.Send(packet);
            }
        }
    }
}