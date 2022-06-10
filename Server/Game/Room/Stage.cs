using System.Collections.Generic;
using System.IO;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class Pos
    {
        public Pos(int y, int z, int x)
        {
            Y = y;
            Z = z;
            X = x;
        }

        public int Y;
        public int Z;
        public int X;
    }

    public class Stage
    {
        public int MinY { get; set; }
        public int MaxY { get; set; }

        public int MinZ { get; set; }
        public int MaxZ { get; set; }

        public int MinX { get; set; }
        public int MaxX { get; set; }

        private int YCount { get; set; }
        private int ZCount { get; set; }
        private int XCount { get; set; }

        private char[,,] _collision;
        private Player[,,] _players;

        private List<Pos> _startRespawn = new List<Pos>();

        public Pos FindStartPos()
        {
            foreach (Pos pos in _startRespawn)
            {
                if (_players[pos.Y, pos.Z, pos.X] == null)
                    return pos;
            }

            return null;
        }

        public void ApplyMove(Player player)
        {
            if (player.Room == null)
                return;
            if (player.Session == null)
                return;

            PositionInfo posInfo = player.PosInfo;
            _players[posInfo.PosY, posInfo.PosZ, posInfo.PosX] = player;
        }

        public void LoadStage(int stageId, string pathPrefix = "../../../../../Shared/StageData")
        {
            string stageName = "Stage_" + stageId.ToString("000");
            string text = File.ReadAllText($"{pathPrefix}/{stageName}/{stageName}_Info.txt");

            StringReader reader = new StringReader(text);

            MinY = int.Parse(reader.ReadLine());
            MaxY = int.Parse(reader.ReadLine());

            MinZ = int.Parse(reader.ReadLine());
            MaxZ = int.Parse(reader.ReadLine());

            MinX = int.Parse(reader.ReadLine());
            MaxX = int.Parse(reader.ReadLine());

            YCount = MaxY - MinY + 1;
            ZCount = MaxZ - MinZ - 1;
            XCount = MaxX - MinX;

            _collision = new char[YCount, ZCount, XCount];
            _players = new Player[YCount, ZCount, XCount];

            for (int y = 0; y < YCount; y++)
            {
                string txt = File.ReadAllText($"{pathPrefix}/{stageName}/{stageName}_Collision_{y}.txt");

                StringReader colReader = new StringReader(txt);
                for (int z = 0; z < ZCount; z++)
                {
                    string line = colReader.ReadLine();
                    for (int x = 0; x < XCount; x++)
                    {
                        _collision[y, z, x] = line[x];

                        if (line[x] == '8')
                        {
                            _startRespawn.Add(new Pos(y, z, x));
                        }
                    }
                }
            }
        }
    }
}