using Google.Protobuf.Protocol;

namespace Server
{
    public class Player
    {
        public ClientSession Session { get; set; }
        public GameRoom Room { get; set; }
        private PlayerInfo _info = new PlayerInfo {PosInfo = new PositionInfo()};

        public PlayerInfo Info
        {
            get { return _info; }
            set { _info = value; }
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
            set { Info.PosInfo = value; }
        }
    }
}