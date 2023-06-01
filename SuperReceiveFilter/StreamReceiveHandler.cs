using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperReceiveFilter
{
    public class StreamReceiveHandler<TPackageInfo> : PipReceiveHandler<TPackageInfo>
    {
        public StreamReceiveHandler(IReceiveFilter<TPackageInfo> pipelineFilter) : base(pipelineFilter)
        {

        }

        private Stream _stream;

        public void BindStream(Stream stream)
        {
            _stream = stream;

        }



        protected override async ValueTask<int> FillPipeWithDataAsync(byte[] bytes, CancellationToken cancellationToken)
        {
            if (_stream == null)
            {
                throw new ArgumentNullException("Network basic stream binding needs to be specified .calling method BindStream");
            }

            return await _stream.ReadAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
        }



        public override void Dispose()
        {
            _stream?.Dispose();
            base.Dispose();
        }
    }
}
