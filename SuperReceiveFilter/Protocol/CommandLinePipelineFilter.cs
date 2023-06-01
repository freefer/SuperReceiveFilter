using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperReceiveFilter.Protocol
{
    /// <summary>
    /// 命令行协议以\r\n结束命令名称和参数用空格隔开
    /// 数据例如：MSG hellow! param1 param2\r\n
    /// </summary>
    public class CommandLinePipelineFilter : TerminatorPipelineFilter<StringPackageInfo>
    {
        protected Encoding Encoding { get; private set; }
        public CommandLinePipelineFilter() : this(Encoding.UTF8)
        {
        }
        public CommandLinePipelineFilter(Encoding encoding) : base(new[] { (byte)'\r', (byte)'\n' })
        {
            this.Encoding = encoding;
        }
        public override StringPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer, int length)
        {
            var data = buffer.Slice(0, buffer.Length - 2).ToArray();
            var text = Encoding.GetString(data);
            var parts = text.Split(new[] { ' ' }, 2);

            var key = parts[0];

            if (parts.Length <= 1)
            {
                return new StringPackageInfo
                {
                    Key = key
                };
            }

            return new StringPackageInfo
            {
                Key = key,
                Body = parts[1],
                Parameters = parts[1].Split(' ')
            };
        }
    }
}
