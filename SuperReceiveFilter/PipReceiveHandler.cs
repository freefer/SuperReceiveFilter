using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperReceiveFilter
{
    public abstract class PipReceiveHandler<TPackageInfo> : IReceiveHandler<TPackageInfo>
    {
        private readonly Pipe pipe;

        private PipeReader reader;
        private PipeWriter writer;

        private Task writing;
        private Task reading;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        public PipReceiveHandler(IReceiveFilter<TPackageInfo> pipelineFilter)
        {

            _pipelineFilter = pipelineFilter;
            pipe = new Pipe();
            reader = pipe.Reader;
            writer = pipe.Writer;

        }

        private bool _initial = false;

        public bool Initial => _initial;
        public long ReadBufferSize { get; set; } = 10240;
        public long MaxPackageLength { get; set; } = 1024 * 1024;

        private readonly IReceiveFilter<TPackageInfo> _pipelineFilter;
        public IReceiveFilter<TPackageInfo> PipelineFilter => _pipelineFilter;

        private bool _ready = false;
        /// <summary>
        /// 开始读取，当连接成功时
        /// </summary>
        public void Begin()
        {
            _ready = true;
        }

        /// <summary>
        /// 停止结束读取，当连接断开时
        /// </summary>
        public void End()
        {
            _ready = false;
        }

        /// <summary>
        /// 启动异步Pip读取
        /// </summary>
        /// <returns></returns>
        public Task StartReadDataAsync()
        {
            if (_initial) return Task.CompletedTask;
            _initial = true;
            return Task.Run(async () =>
            {
                writing = FillPipeAsync();
                reading = ReadPipeAsync();

                await Task.WhenAll(writing, reading).ConfigureAwait(false);
            });

        }


        public event EventHandler<PackageEventArgs<TPackageInfo>> PackageReceived;

        public event EventHandler<PipHandlerErrorEventArgs> PackageError;
        protected abstract ValueTask<int> FillPipeWithDataAsync(byte[] bytes, CancellationToken cancellationToken);


        private async Task FillPipeAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                byte[] buffer = new byte[ReadBufferSize];
                int bytesRead = 0;

                try
                {
                    if (_ready)
                        bytesRead = await FillPipeWithDataAsync(buffer, _cts.Token);

                }
                catch (Exception e)
                {
                    PackageError?.Invoke(this, new PipHandlerErrorEventArgs(e));
                }

                if (bytesRead <= 0)
                {
                    Task.Delay(10).Wait();
                }
                if (bytesRead > 0)
                {
                    await writer.WriteAsync(buffer.AsMemory(0, bytesRead), _cts.Token);
                }
                //Make the data available to the PipeReader.
                FlushResult result = await writer.FlushAsync();

                if (result.IsCompleted)
                {
                    break;
                }
            }
            await writer.CompleteAsync();
        }

        /// <summary>
        /// 读取Pip管道数据
        /// </summary>
        /// <returns></returns>
        private async Task ReadPipeAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                ReadResult result = default;


                result = await reader.ReadAsync(_cts.Token).ConfigureAwait(false);


                var buffer = result.Buffer;

                if (result.IsCanceled)
                {
                    break;
                }

                var completed = result.IsCompleted;

                try
                {
                    while (true)
                    {
                        var isPackageInfo = _pipelineFilter.Filter(ref buffer, out ReadOnlySequence<byte> package, out int offset, out int packetLength);
                        if (packetLength >= MaxPackageLength)
                        {
                            buffer = buffer.Slice(buffer.End);
                            throw new Exception("超出包最大长度");
                        }

                        if (isPackageInfo)
                        {
                            // 切割掉已处理的数据
                            buffer = buffer.Slice(offset + packetLength);
                            var pack = _pipelineFilter.DecodePackage(ref package, packetLength);
                            PackageReceived?.Invoke(this, new PackageEventArgs<TPackageInfo>(pack, package));

                        }
                        else
                        {
                            break;

                        }

                    }

                    if (completed)
                    {
                        break;
                    }
                }

                finally
                {
                    reader.AdvanceTo(buffer.Start, buffer.End);

                }
            }

            await reader.CompleteAsync();
        }




        public virtual void Dispose()
        {
            _cts.Cancel();
            pipe.Writer.CancelPendingFlush();
            pipe.Reader.CancelPendingRead();

        }
    }
}
