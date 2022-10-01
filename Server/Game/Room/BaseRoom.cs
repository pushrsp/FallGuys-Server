using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class BaseRoom : JobDispatcher, IRoom
    {
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
        }
    }
}