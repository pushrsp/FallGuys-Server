using System;

namespace Core
{
    public class RecvBuffer
    {
        private ArraySegment<byte> _buffer;
        private int _writePos;
        private int _readPos;

        public RecvBuffer(int size)
        {
            _buffer = new ArraySegment<byte>(new byte[size], 0, size);
        }

        public int AllocSize
        {
            get => _writePos - _readPos;
        }

        public int FreeSize
        {
            get => _buffer.Count - _writePos;
        }

        public ArraySegment<byte> ReadSegment
        {
            get => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, AllocSize);
        }

        public ArraySegment<byte> WriteSegment
        {
            get => new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize);
        }

        public void Clean()
        {
            int allocSize = AllocSize;
            if (allocSize == 0)
            {
                _readPos = _writePos = 0;
            }
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, allocSize);
                _readPos = 0;
                _writePos = allocSize;
            }
        }

        public bool OnRead(int bytesLen)
        {
            if (bytesLen > AllocSize)
                return false;

            _readPos += bytesLen;
            return true;
        }

        public bool OnWrite(int bytesLen)
        {
            if (bytesLen > FreeSize)
                return false;

            _writePos += bytesLen;
            return true;
        }
    }
}