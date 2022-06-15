using System;
using Core;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;

// ReSharper disable All

public class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.Me;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.HandleMove(player, movePacket);
    }

    public static void C_JumpHandler(PacketSession session, IMessage packet)
    {
        C_Jump jumpPacket = packet as C_Jump;

        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.Me;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;
    }
}