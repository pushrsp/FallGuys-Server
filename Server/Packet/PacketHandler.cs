using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Core;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;

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

        GameRoom room = player.GameRoom;
        if (room == null)
            return;

        room.HandleMove(player, movePacket);
    }

    public static void C_JumpHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.Me;
        if (player == null)
            return;

        GameRoom room = player.GameRoom;
        if (room == null)
            return;

        room.HandleJump(player);
    }

    public static void C_DieHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.Me;
        if (player == null)
            return;

        GameRoom room = player.GameRoom;
        if (room == null)
            return;

        room.HandleDie(player);
    }

    public static void C_LoginHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = packet as C_Login;
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleLogin(loginPacket);
    }

    public static void C_EnterGameHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;

        clientSession.HandleEnterGame();
    }

    public static void C_MakeRoomHandler(PacketSession session, IMessage packet)
    {
        C_MakeRoom makeRoomPacket = packet as C_MakeRoom;
        ClientSession clientSession = session as ClientSession;

        RoomManager.Instance.HandleMakeRoom(makeRoomPacket, clientSession.Me);
    }
}