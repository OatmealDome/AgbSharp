using System;
using System.Collections.Generic;

namespace AgbSharp.Core.Memory
{
    interface IMemoryRegion
    {
        IEnumerable<Tuple<uint, uint>> GetHandledRanges();

        byte Read(uint address);

        void Write(uint address, byte val);

    }
}