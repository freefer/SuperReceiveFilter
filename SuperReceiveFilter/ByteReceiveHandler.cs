using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperReceiveFilter
{
    public class ByteReceiveHandler<TPackageInfo> : PipReceiveHandler<TPackageInfo>
    {
        public ByteReceiveHandler(IReceiveFilter<TPackageInfo> pipelineFilter) : base(pipelineFilter)
        {
        }
        private Func<byte[], CancellationToken, int> _receiveHanlder;
        public void SetupReceive(Func<byte[], CancellationToken, int> acFunc)
        {
            _receiveHanlder = acFunc; ;
        }
        protected override ValueTask<int> FillPipeWithDataAsync(byte[] bytes, CancellationToken cancellationToken)
        {
            return new ValueTask<int>(Task.Run(() => _receiveHanlder(bytes, cancellationToken), cancellationToken));

        }
    }
}
