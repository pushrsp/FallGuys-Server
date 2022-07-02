using System;
using Google.Protobuf.Protocol;

namespace Server.Game.Object
{
    public class RotateObs : Obstacle
    {
        private float _yAngle;
        private long _nextMoveTick;

        public override void Update()
        {
            if (Room == null)
                return;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long) (1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + tick;

            _yAngle += tick * 0.13f * Multiplier;
            if (_yAngle > 360)
                _yAngle = 0;

            S_RotateObstacle rotateObstacle = new S_RotateObstacle();
            rotateObstacle.ObstacleId = Id;
            rotateObstacle.YAngle = _yAngle;

            Room.Broadcast(rotateObstacle);
        }
    }
}