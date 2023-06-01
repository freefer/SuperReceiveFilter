using SuperReceiveFilter.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples
{
    public class MyFixedSizePipelineFilter : FixedSizePipelineFilter<string>
    {
        public MyFixedSizePipelineFilter() : base(10)
        {
        }

        public override string DecodePackage(ref ReadOnlySequence<byte> buffer, int length)
        {
            return Encoding.UTF8.GetString(buffer.ToArray());
        }
    }
}
