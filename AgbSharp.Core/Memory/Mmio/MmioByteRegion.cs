using System;

namespace AgbSharp.Core.Memory.Mmio
{
    internal class MmioByteRegion : MmioRegion<byte>
    {
        public MmioByteRegion(uint baseAddress, Func<byte> readFunc, Action<byte> writeFunc) : base(baseAddress, readFunc, writeFunc)
        {

        }

        public override byte Read(uint address)
        {
            return LastValue;
        }

        public override void Write(uint address, byte val)
        {
            LastValue = val;
        }
        
    }
}