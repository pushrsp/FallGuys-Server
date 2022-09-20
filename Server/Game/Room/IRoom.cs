using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public interface IRoom
    {
        public void HandleEnterRoom(Player player);
        public void HandleMove(Player player, C_Move movePacket);
        public void HandleJump(Player player);
        public void Broadcast(IMessage packet);
    }

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