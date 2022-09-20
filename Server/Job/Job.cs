using System;

namespace Server.Game
{
    public interface IJob
    {
        void Execute();
    }

    public class Job : IJob
    {
        private Action _action;

        public Job(Action action)
        {
            _action = action;
        }

        public void Execute()
        {
            _action.Invoke();
        }
    }

    public class Job<T1> : IJob
    {
        private Action<T1> _action;
        private T1 _t1;

        public Job(Action<T1> action, T1 t1)
        {
            _action = action;
            _t1 = t1;
        }

        public void Execute()
        {
            _action.Invoke(_t1);
        }
    }

    public class Job<T1, T2> : IJob
    {
        private Action<T1, T2> _action;
        private T1 _t1;
        private T2 _t2;

        public Job(Action<T1, T2> action, T1 t1, T2 t2)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
        }

        public void Execute()
        {
            _action.Invoke(_t1, _t2);
        }
    }

    public class Job<T1, T2, T3> : IJob
    {
        private Action<T1, T2, T3> _action;
        private T1 _t1;
        private T2 _t2;
        private T3 _t3;

        public Job(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
        }

        public void Execute()
        {
            _action.Invoke(_t1, _t2, _t3);
        }
    }
}