using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperReceiveFilter.Protocol
{
    /// <summary>
    /// 结束符协议 例如, 一个协议使用两个字符 "##" 作为结束符
    /// 数据例如ABCDEFG##HELLOWWORD##
    /// </summary>
    /// <typeparam name="TPackageInfo"></typeparam>
    public abstract class TerminatorPipelineFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>
    {
        private readonly byte[] _terminator;

        protected TerminatorPipelineFilter(byte[] terminator)
        {
 
            _terminator = terminator;
        }

        public bool Filter(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> package, out int offset,out int packetLength)
        {
            package = default;
            var markLength = _terminator.Length;
            packetLength = 0;
            offset = 0;
            foreach (var segment in buffer)
            {
                var span = segment.Span;
                var matchIndex = span.IndexOf(_terminator);
                if (matchIndex < 0) continue;
                packetLength = matchIndex + markLength;
                package = buffer.Slice(0, packetLength);
     
                return true;
            }
            return false;
        }

        public abstract TPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer, int length);

    }
}
