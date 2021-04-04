using System;

namespace AgbSharp.Core.Memory.Mmio
{
    internal class MmioWordRegion : MmioRegion<uint>
    {
        public MmioWordRegion(uint baseAddress, Func<uint> readFunc, Action<uint> writeFunc) : base(baseAddress, readFunc, writeFunc)
        {

        }

        public override byte Read(uint address)
        {
            return (byte)((LastValue >> BitShiftForAddress(address)) & 0xFF);
        }

        public override void Write(uint address, byte val)
        {
            int bitshift = BitShiftForAddress(address);

            uint mask = (uint)~(0xFF << bitshift);

            LastValue = (LastValue & mask) | (uint)(val << bitshift);
        }

    }
}