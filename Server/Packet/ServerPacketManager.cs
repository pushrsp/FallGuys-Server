using Google.Protobuf;
using Google.Protobuf.Protocol;
using Core;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();

	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }
		
	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CMove, MakePacket<C_Move>);
		_handler.Add((ushort)MsgId.CMove, PacketHandler.C_MoveHandler);		
		_onRecv.Add((ushort)MsgId.CJump, MakePacket<C_Jump>);
		_handler.Add((ushort)MsgId.CJump, PacketHandler.C_JumpHandler);		
		_onRecv.Add((ushort)MsgId.CDie, MakePacket<C_Die>);
		_handler.Add((ushort)MsgId.CDie, PacketHandler.C_DieHandler);		
		_onRecv.Add((ushort)MsgId.CLogin, MakePacket<C_Login>);
		_handler.Add((ushort)MsgId.CLogin, PacketHandler.C_LoginHandler);		
		_onRecv.Add((ushort)MsgId.CEnterGame, MakePacket<C_EnterGame>);
		_handler.Add((ushort)MsgId.CEnterGame, PacketHandler.C_EnterGameHandler);		
		_onRecv.Add((ushort)MsgId.CMakeRoom, MakePacket<C_MakeRoom>);
		_handler.Add((ushort)MsgId.CMakeRoom, PacketHandler.C_MakeRoomHandler);		
		_onRecv.Add((ushort)MsgId.CEnterRoom, MakePacket<C_EnterRoom>);
		_handler.Add((ushort)MsgId.CEnterRoom, PacketHandler.C_EnterRoomHandler);		
		_onRecv.Add((ushort)MsgId.CChangePlayer, MakePacket<C_ChangePlayer>);
		_handler.Add((ushort)MsgId.CChangePlayer, PacketHandler.C_ChangePlayerHandler);		
		_onRecv.Add((ushort)MsgId.CStartGame, MakePacket<C_StartGame>);
		_handler.Add((ushort)MsgId.CStartGame, PacketHandler.C_StartGameHandler);		
		_onRecv.Add((ushort)MsgId.CEnterGameRoom, MakePacket<C_EnterGameRoom>);
		_handler.Add((ushort)MsgId.CEnterGameRoom, PacketHandler.C_EnterGameRoomHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
        {
            CustomHandler.Invoke(session, pkt, id);
        }
        else
        {
            Action<PacketSession, IMessage> action;
            if (_handler.TryGetValue(id, out action))
                action.Invoke(session, pkt);
        }
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}