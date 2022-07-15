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

        IRoom room = player.Room;
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

        IRoom room = player.Room;
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

    public static void C_EnterRoomHandler(PacketSession session, IMessage packet)
    {
        C_EnterRoom enterRoomPacket = packet as C_EnterRoom;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.Me;
        if (player == null)
            return;

        IRoom room = RoomManager.Instance.GetRoom(enterRoomPacket.RoomIdx);
        room.HandleEnterRoom(player);
    }

    public static void C_ChangePlayerHandler(PacketSession session, IMessage packet)
    {
        C_ChangePlayer changePlayerPacket = packet as C_ChangePlayer;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.Me;
        if (player == null)
            return;

        Room room = player.EnteredRoom;
        if (room == null)
            return;

        room.HandleChangePlayer(player, changePlayerPacket);
    }

    public static void C_StartGameHandler(PacketSession session, IMessage packet)
    {
        C_StartGame startGamePacket = packet as C_StartGame;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.Me;
        if (player == null)
            return;

        Room room = player.EnteredRoom;
        if (room == null)
            return;

        room.HandleStartGame(player, startGamePacket.StageId);
    }

    public static void C_EnterGameRoomHandler(PacketSession session, IMessage packet)
    {
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.Me;
        if (player == null)
            return;

        IRoom room = player.Room;
        if (room == null)
            return;

        room.HandleEnterRoom(player);
    }
}