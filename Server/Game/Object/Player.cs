using Google.Protobuf.Protocol;

namespace Server
{
    public class Player
    {
        public ClientSession Session { get; set; }
        public GameRoom Room { get; set; }
        private PlayerInfo _info = new PlayerInfo {PosInfo = new PositionInfo(), MoveDir = new PositionInfo()};

        public PlayerInfo Info
        {
            get { return _info; }
            set { _info = value; }
        }

        public PlayerState State
        {
            get { return Info.State; }
            set { Info.State = value; }
        }

        public float Speed
        {
            get { return Info.Speed; }
            set { Info.Speed = value; }
        }

        public string Name
        {
            get { return Info.Name; }
            set { Info.Name = value; }
        }

        public int ObjectId
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }

        public PositionInfo PosInfo
        {
            get { return Info.PosInfo; }
            set
            {
                Info.PosInfo.PosX = value.PosX;
                Info.PosInfo.PosY = value.PosY;
                Info.PosInfo.PosZ = value.PosZ;
            }
        }

        public PositionInfo MoveDir
        {
            get { return Info.MoveDir; }
            set
            {
                Info.MoveDir.PosX = value.PosX;
                Info.MoveDir.PosY = value.PosY;
                Info.MoveDir.PosZ = value.PosZ;
            }
        }
    }
}