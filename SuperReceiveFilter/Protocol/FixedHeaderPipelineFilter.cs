using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SuperReceiveFilter.Protocol
{
    /// <summary>
    /// 头部格式固定并且包含内容长度的协议
    /// +-------+---+-------------------------------+
    /// |request| l |                               |
    /// | type  | e |    request body               |
    /// |  (1)  | n |                               |
    /// |       |(2)|                               |
    /// +-------+---+-------------------------------+
    /// </summary>
    /// <typeparam name="TPackageInfo"></typeparam>
    public abstract class FixedHeaderPipelineFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>
    {
        private int _headerLenth = 0;
        private bool _foundHeader;
        private int _totalSize;
        protected FixedHeaderPipelineFilter(int headerLenth)
        {
            _headerLenth = headerLenth;
        }

        protected abstract int GetBodyLengthFromHeader(ref byte[] buffer);
        public bool Filter(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> package, out int offset, out int packetLength)
        {
            package = default;
            packetLength = 0;
            offset = 0;
            if (!_foundHeader)
            {
                while (true)
                {
                    if (buffer.Length < _headerLenth)
                    {
                        return false;
                    }

                    var header = buffer.Slice(0, _headerLenth).ToArray();
                    var bodyLength = GetBodyLengthFromHeader(ref header);
                    _totalSize = _headerLenth + bodyLength;
                    packetLength = _totalSize;
                    if (bodyLength < 0)
                    {
                        buffer = buffer.Slice(1);
                        continue;
                    }

                    if (bodyLength == 0)
                    {
                        package = buffer.Slice(0, _totalSize);
                        return true;
                    }

                    _foundHeader = true;
                    break;
                }
            }

            if (buffer.Length < _totalSize)
                return false;
            package = buffer.Slice(0, _totalSize);
            packetLength = _totalSize;
            _foundHeader = false;
            _totalSize = 0;
            return true;
        }

        public abstract TPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer, int length);

    }
}
