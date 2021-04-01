using System;
using System.Collections.Generic;

namespace AgbSharp.Core.Memory.Mmio
{
    internal class MmioRegion : IMemoryRegion
    {
        private readonly Func<byte> ReadFunc;
        private readonly Action<byte> WriteFunc;

        public MmioRegion(Func<byte> readFunc, Action<byte> writeFunc)
        {
            ReadFunc = readFunc;
            WriteFunc = writeFunc;
        }

        public IEnumerable<Tuple<uint, uint>> GetHandledRanges()
        {
            throw new InvalidOperationException("MMIO region cannot be manually registered");
        }

        public byte Read(uint address)
        {
            return ReadFunc();
        }

        public void Write(uint address, byte val)
        {
            WriteFunc(val);
        }

    }
}
