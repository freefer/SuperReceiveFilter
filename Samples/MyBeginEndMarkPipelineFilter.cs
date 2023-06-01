using System;
using System.Buffers;
using System.Linq;
using SuperReceiveFilter;
using SuperReceiveFilter.Protocol;

namespace Samples
{
    public class MyBeginEndMarkPipelineFilter : BeginEndMarkPipelineFilter<BytePackage>
    {
        private static byte[] _beginMark = new byte[] { 0xBB, 0x55 };
        private static byte[] _endMark = new byte[] { 0X7E, 0X7E };
        public MyBeginEndMarkPipelineFilter() : base(_beginMark, _endMark)
        {
        }


        public override BytePackage DecodePackage(ref ReadOnlySequence<byte> buffer, int length)
        {
            /*
             *解析数据范围自己想要得包体数据
             *
             */
            var data = buffer.ToArray();
         
            var package = new BytePackage();
            package.Key = data[2].ToString();
            package.Body = data.Skip(3).Take(data.Length - 5).ToArray();
            return package;
        }
    }
}