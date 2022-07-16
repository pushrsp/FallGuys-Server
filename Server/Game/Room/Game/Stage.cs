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
            return new Vector3Int((int) MathF.Round(x, 0),
                (int) MathF.Round(y, 0),
                (int) MathF.Round(z, 0));
        }

        public static Vector3Int FloorToInt(float x, float y, float z)
        {
            return new Vector3Int((int) MathF.Floor(x),
                (int) MathF.Floor(y),
                (int) MathF.Floor(z));
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

        public void ApplyLeave(PositionInfo posInfo)
        {
            if (!IsValidate(posInfo))
                return;

            Tuple<int, int, int> pos = ConvertPosToIndex(posInfo);

            _players[pos.Item1, pos.Item2, pos.Item3] = null;
        }

        public void ApplyMove(Player player)
        {
            PositionInfo posInfo = player.PosInfo;
            if (!IsValidate(posInfo))
                return;

            Tuple<int, int, int> pos = ConvertPosToIndex(posInfo);

            _players[pos.Item1, pos.Item2, pos.Item3] = player;
        }

        public void ApplyMove(Player player, PositionInfo destPos)
        {
            Tuple<int, int, int> source = ConvertPosToIndex(player.PosInfo);
            _players[source.Item1, source.Item2, source.Item3] = null;

            Tuple<int, int, int> dest = ConvertPosToIndex(destPos);
            _players[dest.Item1, dest.Item2, dest.Item3] = player;

            // Console.WriteLine($"ROUND: ({dest.Item1}, {dest.Item2}, {dest.Item3})");
        }

        private Tuple<int, int, int> ConvertPosToIndex(PositionInfo posInfo)
        {
            Vector3Int pos = Vector3Int.RoundToInt(posInfo.PosX, posInfo.PosY, posInfo.PosZ);
            int y = pos.y - MinY;
            int z = MaxZ - pos.z;
            int x = pos.x - MinX;

            return new Tuple<int, int, int>(y, z, x);
        }

        private Tuple<int, int, int> ConvertIndexToPos(int x, int y, int z)
        {
            return new Tuple<int, int, int>(y + MinY, MaxZ - z, x + MinX + 1);
        }

        public Tuple<int, int, int> FindRespawn(PositionInfo posInfo, Player player)
        {
            //위 왼쪽 아래 오른쪽 뒤 뒤
            int[] deltaY = new int[6] {-1, 0, 1, 1, 0, 0};
            int[] deltaX = new int[6] {0, -1, 0, 1, 0, 0};
            int[] deltaZ = new int[6] {0, 0, 0, 0, 1, 1};

            Queue<Tuple<int, int, int>> q = new Queue<Tuple<int, int, int>>();
            q.Enqueue(ConvertPosToIndex(posInfo));
            bool[,,] visited = new bool[YCount, ZCount, XCount];

            while (q.Count > 0)
            {
                Tuple<int, int, int> pos = q.Dequeue();

                if (_collision[pos.Item1, pos.Item2, pos.Item3] == '8')
                {
                    _players[pos.Item1, pos.Item2, pos.Item3] = player;
                    return ConvertIndexToPos(pos.Item3, pos.Item1, pos.Item2);
                }

                for (int i = 0; i < 6; i++)
                {
                    int nextY = pos.Item1 + deltaY[i];
                    int nextZ = pos.Item2 + deltaZ[i];
                    int nextX = pos.Item3 + deltaX[i];

                    if (nextY < 0 || nextY >= YCount)
                        continue;
                    if (nextZ < 0 || nextZ >= ZCount)
                        continue;
                    if (nextX < 0 || nextX >= XCount)
                        continue;
                    if (_collision[nextY, nextZ, nextX] == '4')
                        continue;
                    if (_players[nextY, nextZ, nextX] != null)
                        continue;
                    if (visited[nextY, nextZ, nextX])
                        continue;

                    visited[nextY, nextZ, nextX] = true;
                    q.Enqueue(new Tuple<int, int, int>(nextY, nextZ, nextX));
                }
            }

            return null;
        }

        public char CanGo(PositionInfo posInfo)
        {
            if (!IsValidate(posInfo))
                return '-';

            Tuple<int, int, int> pos = ConvertPosToIndex(posInfo);

            return _collision[pos.Item1, pos.Item2, pos.Item3];
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

                        if (line[x] == 'a')
                            GameManager.Instance.Find(1).Add<RotateObs>(40.0f, new Vector3(0, 0, 0));
                        if (line[x] == 'b')
                        {
                            Tuple<int, int, int> pos = ConvertIndexToPos(x, y, z);
                            GameManager.Instance.Find(1)
                                .Add<PendulumObs>(10.0f, new Vector3(pos.Item3, pos.Item1, pos.Item2));
                        }

                        if (line[x] == '8')
                            _startRespawn.Add(new Pos(y, MaxZ - z, MinX + x + 1));
                    }
                }
            }
        }
    }
}