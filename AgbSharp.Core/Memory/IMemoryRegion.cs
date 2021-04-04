using System;
using System.Collections.Generic;

namespace AgbSharp.Core.Memory
{
    public interface IMemoryRegion
    {
        IEnumerable<Tuple<uint, uint>> GetHandledRanges();

        byte Read(uint address);

        void Write(uint address, byte val);

    }
}