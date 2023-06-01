using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperReceiveFilter.Protocol
{
    /// <summary>
    /// 换行符协议 \r\n 结束符
    /// </summary>
    public class LinePipelineFilter : TerminatorPipelineFilter<string>
    {
        protected Encoding Encoding { get; private set; }

        public LinePipelineFilter() : this(Encoding.UTF8)
        {

        }

        public LinePipelineFilter(Encoding encoding) : base(new[] { (byte)'\r', (byte)'\n' })
        {
            Encoding = encoding;
        }


        public override string DecodePackage(ref ReadOnlySequence<byte> buffer, int length)
        {
            var data = buffer.Slice(0, length - 2).ToArray();
            return Encoding.GetString(data);
        }
    }

}
