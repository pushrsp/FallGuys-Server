using System;
using Google.Protobuf.Protocol;

namespace Server.Game.Object
{
    public class RotateObs : Obstacle
    {
        private float _yAngle;
        private long _nextMoveTick;
        private int _offset = 0;

        public override void Update()
        {
            if (Room == null)
                return;

            if (_offset == 0)
                _offset = RotateDir == Dir.Left ? -1 : 1;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long) (1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + tick;

            _yAngle += tick * 0.13f * _offset;
            if (_yAngle > 360)
                _yAngle = 0;

            S_RotateObstacle rotateObstacle = new S_RotateObstacle();
            rotateObstacle.ObstacleId = Id;
            rotateObstacle.YAngle = _yAngle;

            Room.Broadcast(rotateObstacle);
        }
    }
}