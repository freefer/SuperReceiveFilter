using System;
using System.Buffers;

namespace SuperReceiveFilter
{
    public class PackageEventArgs<TPackageInfo> : EventArgs
    {
        public TPackageInfo Package { get; private set; }
        public ReadOnlySequence<byte> Bytes { get; }

        public PackageEventArgs(TPackageInfo package, ReadOnlySequence<byte> bytes)
        {
            this.Package = package;
            Bytes = bytes;
        }
    }
}