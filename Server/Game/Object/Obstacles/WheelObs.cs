namespace Server.Game.Object
{
    public class WheelObs
    {
        public GameRoom Room { get; set; }
        public float Speed { get; set; }

        private float _runningTime = 0.0f;
        private long _nextMoveTick = 0;
    }
}