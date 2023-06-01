using SuperReceiveFilter.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Samples
{
    public class MyFixedHeaderPipelineFilter : FixedHeaderPipelineFilter<BytePackage>
    {
        public MyFixedHeaderPipelineFilter() : base(3)
        {
        }

        protected override int GetBodyLengthFromHeader(ref byte[] buffer)
        {
            var data = buffer.ToArray();
            if (data[0]!=0xBB)
            {
                return -1;
            }
            return data[2];
        }

        public override BytePackage DecodePackage(ref ReadOnlySequence<byte> buffer, int length)
        { 
            var data = buffer.ToArray();

            return new BytePackage()
            {
                Key = data[0].ToString("X2"),
                Body = data.Skip(3).ToArray(),
            };

        }
    }
}
