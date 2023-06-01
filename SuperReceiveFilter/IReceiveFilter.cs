using System.Buffers;

namespace SuperReceiveFilter
{
    public interface IReceiveFilter<out TPackageInfo>
    {
        bool Filter(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> package, out int offset, out int packetLength);
        TPackageInfo DecodePackage(ref ReadOnlySequence<byte> buffer,int length);
    }
}