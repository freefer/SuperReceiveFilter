using SuperReceiveFilter.Protocol;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class TextTerminatorPipelineFilter : TerminatorPipelineFilter<TextPackage>
    {
        private static readonly byte[] Mark = new byte[] { (byte)'#', (byte)'#' };
        public TextTerminatorPipelineFilter() : base(Mark)
        {

        }

        public override TextPackage DecodePackage(ref ReadOnlySequence<byte> buffer, int length)
        {
            var data = buffer.Slice(0, length - 2).ToArray();
            return new TextPackage() { Text = Encoding.Default.GetString(data) };
        }
    }
}
