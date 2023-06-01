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
    /// 固定请求大小的协议
    /// 如果你的每个请求都是有固定大小个字符组成的 可以使用以下过滤器
    /// </summary>
    /// <typeparam name="TPackageInfo"></typeparam>
    public abstract class FixedSizePipelineFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>
    {
        private int _size;
        public FixedSizePipelineFilter(int size)
        {
            _size = size;
        }
        public bool Filter(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> package,out int offset, out int packetLength)
        {
            package = default;
            packetLength = 0;
            offset = 0;
            if (buffer.Length < _size)
            {
                return false;
            }

            packetLength = _size;
            package = buffer.Slice(0, _size);
            return true;
        }

        public abstract TPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer, int length);

    }
}
