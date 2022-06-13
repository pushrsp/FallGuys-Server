using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Channels;
using Google.Protobuf.Protocol;
using Server.Game.Object;

namespace Server.Game
{
    public struct Vector3Int
    {
        public int y;
        public int z;
        public int x;

        public Vector3Int(int x, int y, int z)
        {
            this.y = y;
            this.z = z;
            this.x = x;
        }

        public static Vector3Int RoundToInt(Vector3 pos)
        {
            return new Vector3Int((int) MathF.Round(pos.x + 0.1f, 0),
                (int) MathF.Round(pos.y + 0.1f, 0),
                (int) MathF.Round(pos.z + 0.1f, 0));
        }

        public static Vector3Int RoundToInt(float x, float y, float z)
        {
            return new Vector3Int((int) MathF.Round(x + 0.1f, 0),
                (int) MathF.Round(y + 0.1f, 0),
                (int) MathF.Round(z + 0.1f, 0));
        }
    }

    public struct Vector3
    {
        public float y;
        public float z;
        public float x;

        public Vector3(float x, float y, float z)
        {
            this.y = y;
            this.z = z;
            this.x = x;
        }

        public static Vector3 up
        {
            get => new Vector3(0, 1, 0);
        }

        public static Vector3 down
        {
            get => new Vector3(0, -1, 0);
        }

        public static Vector3 left
        {
            get => new Vector3(-1, 0, 0);
        }

        public static Vector3 right
        {
            get => new Vector3(1, 0, 0);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public float magnitude
        {
            get { return MathF.Sqrt(sqrMagnitude); }
        }

        public float sqrMagnitude
        {
            get { return x * x + y * y + z * z; }
        }
    }

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
                if (_players[pos.Y - MinY, MaxZ - pos.Z, pos.X - MinX] == null)
                    return pos;
            }

            return null;
        }

        bool IsValidate(PositionInfo posInfo)
        {
            if (posInfo.PosX < MinX || posInfo.PosX > MaxX)
                return false;
            if (posInfo.PosZ < MinZ || posInfo.PosZ > MaxZ)
                return false;
            if (posInfo.PosY < MinY || posInfo.PosY > MaxY)
                return false;

            return true;
        }

        public bool HitPlayer(PositionInfo destPos, int objectId)
        {
            if (!IsValidate(destPos))
                return false;

            Tuple<int, int, int> dest = GetPos(destPos);
            if (_players[dest.Item1, dest.Item2, dest.Item3] != null)
            {
                if (_players[dest.Item1, dest.Item2, dest.Item3].ObjectId != objectId)
                    return true;
            }

            return false;
        }

        public void ApplyLeave(PositionInfo posInfo)
        {
            if (!IsValidate(posInfo))
                return;

            Tuple<int, int, int> pos = GetPos(posInfo);

            _players[pos.Item1, pos.Item2, pos.Item3] = null;
        }

        public void ApplyMove(Player player)
        {
            PositionInfo posInfo = player.PosInfo;
            if (!IsValidate(posInfo))
                return;

            Tuple<int, int, int> pos = GetPos(posInfo);

            _players[pos.Item1, pos.Item2, pos.Item3] = player;
        }

        public void ApplyMove(Player player, PositionInfo destPos)
        {
            Tuple<int, int, int> source = GetPos(player.PosInfo);
            _players[source.Item1, source.Item2, source.Item3] = null;

            Tuple<int, int, int> dest = GetPos(destPos);
            _players[dest.Item1, dest.Item2, dest.Item3] = player;
        }

        public Tuple<int, int, int> GetPos(PositionInfo posInfo)
        {
            Vector3Int pos = Vector3Int.RoundToInt(posInfo.PosX, posInfo.PosY, posInfo.PosZ);
            int y = pos.y - MinY;
            int z = MaxZ - pos.z;
            int x = pos.x - MinX;

            return new Tuple<int, int, int>(y, z, x);
        }

        public int CanGo(PositionInfo posInfo)
        {
            if (!IsValidate(posInfo))
                return -1;

            Tuple<int, int, int> pos = GetPos(posInfo);

            switch (_collision[pos.Item1, pos.Item2, pos.Item3])
            {
                case '0':
                    return 0;
                case '3':
                    return 3;
                case '4':
                    return -1;
                case '5':
                    return -1;
                case '6':
                    return 0;
                case '7':
                    return 0;
                case '8':
                    return 0;
                default:
                    return -1;
            }
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

                        if (line[x] == '7')
                            RoomManager.Instance.Find(1).AddRotateObs();

                        if (line[x] == '8')
                        {
                            _startRespawn.Add(new Pos(y, MaxZ - z, MinX + x + 1));
                        }
                    }
                }
            }
        }
    }
}