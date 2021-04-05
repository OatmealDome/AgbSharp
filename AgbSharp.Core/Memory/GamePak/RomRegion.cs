using System;
using System.Collections.Generic;

namespace AgbSharp.Core.Memory.GamePak
{
    class RomRegion : IMemoryRegion
    {
        public const uint REGION_ONE_START = 0x08000000;
        public const uint REGION_TWO_START = 0x0A000000;
        public const uint REGION_THREE_START = 0x0C000000;
        public const uint REGION_SIZE = 0x2000000;
        public const uint MASK = 0x1FFFFFF;

        private byte[] Data;

        public RomRegion(byte[] data)
        {
            Data = data;
        }

        public IEnumerable<Tuple<uint, uint>> GetHandledRanges()
        {
            return new List<Tuple<uint, uint>>()
            {
                new Tuple<uint, uint>(REGION_ONE_START, REGION_SIZE),
                new Tuple<uint, uint>(REGION_TWO_START, REGION_SIZE),
                new Tuple<uint, uint>(REGION_THREE_START, REGION_SIZE)
            };
        }

        public byte Read(uint address)
        {
            return Data[address & MASK];
        }

        public void Write(uint address, byte val)
        {
            Data[address & MASK] = val;
        }

    }
}
