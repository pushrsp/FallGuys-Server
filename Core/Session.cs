using System.Net.Sockets;

namespace Core
{
    public abstract class Session
    {
        private Socket _socket;

        private SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();
        private SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += OnRecvCompleted;
            _sendArgs.Completed += OnSendCompleted;
        }

        #region 통신

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
        }

        #endregion
    }
}