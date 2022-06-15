using System;
using Google.Protobuf.Protocol;

namespace Server.Game.Object
{
    public class RotateObs
    {
        public GameRoom Room { get; set; }
        public float Speed { get; set; }

        private float _runningTime = 0.0f;
        private long _nextMoveTick = 0;

        public void Update()
        {
            if (Room == null)
                return;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long) (1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + tick;

            _runningTime += tick * 0.13f;
            if (_runningTime > 360)
                _runningTime = 0;
        }
    }
}