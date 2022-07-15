using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Core;
using Google.Protobuf.Protocol;
using Server.Game;
using Server.Game.Object;

namespace Server
{
    public partial class ClientSession
    {
        public void HandleLogin(C_Login loginPacket)
        {
            if (loginPacket.Token == null)
                return;

            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                JwtSecurityToken token = handler.ReadJwtToken(loginPacket.Token);

                string id = token.Claims.First(t => t.Type == "id").Value;
                string username = token.Claims.First(t => t.Type == "username").Value;

                S_Login loginOk = new S_Login();
                loginOk.Success = true;
                loginOk.Id = id;
                loginOk.Username = username;

                Me = new Player();
                {
                    Me.ObjectId = id;
                    Me.Username = username;
                    Me.Session = this;
                    Me.Token = loginPacket.Token;
                    Me.GameState = GameState.Lobby;
                }

                Send(loginOk);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Token Error: {e.Message}");

                S_Login loginOk = new S_Login();
                loginOk.Success = true;

                Send(loginOk);
            }
        }

        public void HandleEnterGame()
        {
            RoomManager.Instance.EnterRoom(Me);
            // GameRoom room = GameManager.Instance.Find(1);

            // {
            // Pos pos = room.Stage.FindStartPos();

            // Me.Session = this;
            // Me.Name = $"{Username}";
            // Me.PosInfo.PosY = pos.Y;
            // Me.PosInfo.PosZ = pos.Z;
            // Me.PosInfo.PosX = pos.X;
            // Me.Info.State = PlayerState.Idle;
            // Me.Info.PlayerSelect = _random.Next(1, 11);
            // Me.Info.Speed = 6.0f;
            // }

            // room.EnterRoom(Me);
        }
    }
}