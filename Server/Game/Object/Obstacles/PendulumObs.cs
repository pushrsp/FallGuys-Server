using System;
using Google.Protobuf.Protocol;

namespace Server.Game.Object
{
    public class PendulumObs : Obstacle
    {
        public Vector3 Pivot { get; set; }

        private long _nextMoveTick;
        private float _angleV;
        private float _angleA;
        private float _angle = 3.14f / 4;
        private float _gravity = 1.0f;
        private int _len = 7;

        public override void Update()
        {
            if (Room == null)
                return;

            if (_nextMoveTick >= Environment.TickCount64)
                return;

            long tick = (long) (1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + tick;

            //진자 운동
            float force = _gravity * MathF.Sin(_angle);
            _angleA = force / _len;
            _angleV += _angleA;
            _angle += _angleV;

            Vector3 pos = new Vector3(
                (_len * MathF.Sin(_angle) + Pivot.x) * Multiplier,
                _len * MathF.Cos(_angle) + Pivot.y,
                Pivot.z
            );
            S_PendulumObstacle pendulumObstacle = new S_PendulumObstacle {PosInfo = new PositionInfo()};
            pendulumObstacle.ObstacleId = Id;
            pendulumObstacle.PosInfo.PosX = pos.x;
            pendulumObstacle.PosInfo.PosY = pos.y;
            pendulumObstacle.PosInfo.PosZ = pos.z;

            Room.Broadcast(pendulumObstacle);
        }
    }
}