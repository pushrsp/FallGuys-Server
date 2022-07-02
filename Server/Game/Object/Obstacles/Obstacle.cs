using Google.Protobuf.Protocol;

namespace Server.Game.Object
{
    public class Obstacle
    {
        public GameRoom Room { get; set; }
        public float Speed { get; set; }
        public ObstacleType Type { get; set; }

        public enum Dir
        {
            Left,
            Right
        }

        public int Id { get; set; }

        protected int Multiplier;

        private Dir _rotateDir;

        public Dir RotateDir
        {
            get { return _rotateDir; }
            set
            {
                _rotateDir = value;
                if (value == Dir.Left)
                    Multiplier = -1;
                else
                    Multiplier = 1;
            }
        }

        public virtual void Update()
        {
        }
    }
}