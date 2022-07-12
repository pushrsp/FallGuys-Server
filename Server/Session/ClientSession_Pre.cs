using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Core;
using Google.Protobuf.Protocol;
using Server.Game;
using Server.Game.Object;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public string Token { get; set; }
        public string Id { get; set; }
        public string Username { get; set; }

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

                {
                    Token = loginPacket.Token;
                    Id = id;
                    Username = username;
                }


                S_Login loginOk = new S_Login();
                loginOk.Success = true;
                loginOk.Id = Id;
                loginOk.Username = Username;

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
            Me = RoomManager.Instance.EnterRoom(Username, Id, this);
            // GameRoom room = GameManager.Instance.Find(1);

            {
                // Pos pos = room.Stage.FindStartPos();

                // Me.Session = this;
                // Me.Name = $"{Username}";
                // Me.PosInfo.PosY = pos.Y;
                // Me.PosInfo.PosZ = pos.Z;
                // Me.PosInfo.PosX = pos.X;
                // Me.Info.State = PlayerState.Idle;
                // Me.Info.PlayerSelect = _random.Next(1, 11);
                // Me.Info.Speed = 6.0f;
            }

            // room.EnterRoom(Me);
        }
    }
}