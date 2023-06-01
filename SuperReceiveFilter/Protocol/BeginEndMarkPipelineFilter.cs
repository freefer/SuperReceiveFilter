using System;
using System.Buffers;

namespace SuperReceiveFilter.Protocol
{
    /// <summary>
    /// 带起止符的协议它的所有消息都遵循这种格式 "!xxxxxxxxxxxxxx$"。因此，在这种情况下， "!" 是开始标记， "$" 是结束标记，于是你的接受过滤器可以定义成这样
    /// 0XBB 0X55 .........................0X7E 0X7E
    /// </summary>
    /// <typeparam name="TPackageInfo"></typeparam>
    public abstract class BeginEndMarkPipelineFilter<TPackageInfo> : IReceiveFilter<TPackageInfo>
    {
        private readonly byte[] _beginMark;

        private readonly byte[] _endMark;

        protected BeginEndMarkPipelineFilter(byte[] beginMark, byte[] endMark)
        {
            _beginMark = beginMark;
            _endMark = endMark;
        }

        public bool Filter(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> package, out int offset, out int packetLength)
        {
            package = default;
            bool foundHeader = false;
            bool foundFooter = false;
            int headerIndex = 0;
            packetLength = 0;
            offset = 0;
            // 缓存 start.Length 和 end.Length
            int startLength = _beginMark.Length;
            int endLength = _endMark.Length;

            // 将 Span 提到循环外部来避免多次内存拷贝，
            // 使用 ref 来减少将 ReadOnlyMemory<byte> 转换为 ReadOnlySpan<byte> 的开销
            ReadOnlySpan<byte> span = default;

            foreach (var segment in buffer)
            {
                span = segment.Span;

                if (!foundHeader)
                {
                    headerIndex = span.IndexOf(_beginMark);
                    if (headerIndex >= 0)
                    {
                        foundHeader = true;
                        span = span.Slice(headerIndex + startLength);
                        packetLength += startLength;  // 更新包长
                    }
                }

                if (foundHeader && !foundFooter)
                {
                    int footerIndex = span.IndexOf(_endMark);
                    if (footerIndex >= 0)
                    {
                        foundFooter = true;
                        packetLength += footerIndex + endLength; // 更新包长
                    }
                    else
                    {
                        packetLength += span.Length; // 更新包长
                    }
                }
            }
            //没有找到包头并且数据长度大于包头 视为无效数据
            if (!foundHeader && buffer.Length >= startLength)
            {
                buffer = buffer.Slice(buffer.End);
            }
            // 匹配到包头和包尾
            if (foundHeader && foundFooter)
            {
                package = buffer.Slice(headerIndex, packetLength);
                offset = headerIndex;
                //// 切割掉已处理的数据
                //buffer = buffer.Slice(headerIndex + packetLength);
                return true;
            }

            return false;
        }

        public abstract TPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer, int length);
    }
}