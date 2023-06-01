using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SuperReceiveFilter
{
    public interface IReceiveHandler<TPackageInfo> : IDisposable
    {
        void Begin();
        void End();
        bool Initial { get; }
        long ReadBufferSize { get; set; }
        long MaxPackageLength { get; set; }

        IReceiveFilter<TPackageInfo> PipelineFilter { get; }
        Task StartReadDataAsync();

        event EventHandler<PackageEventArgs<TPackageInfo>> PackageReceived;

    }
}
