using System;

namespace AgbSharp.Core.Memory.Mmio
{
    internal class MmioHalfWordRegion : MmioRegion<ushort>
    {
        public MmioHalfWordRegion(uint baseAddress, Func<ushort> readFunc, Action<ushort> writeFunc) : base(baseAddress, readFunc, writeFunc)
        {

        }

        public override byte Read(uint address)
        {
            return (byte)((LastValue >> BitShiftForAddress(address)) & 0xFF);
        }

        public override void Write(uint address, byte val)
        {
            int bitshift = BitShiftForAddress(address);

            int mask = ~(0xFF << bitshift);

            LastValue = (ushort)((LastValue & mask) | (val << bitshift));
        }

    }
}