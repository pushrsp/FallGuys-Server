using Core;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;

public class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;
    }
}