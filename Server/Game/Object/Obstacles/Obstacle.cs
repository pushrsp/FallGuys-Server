namespace Server.Game.Object
{
    public class Obstacle
    {
        public GameRoom Room { get; set; }
        public float Speed { get; set; }

        public enum Dir
        {
            Left,
            Right
        }

        public Dir RotateDir { get; set; }
        public int Id { get; set; }

        public virtual void Update()
        {
        }
    }
}