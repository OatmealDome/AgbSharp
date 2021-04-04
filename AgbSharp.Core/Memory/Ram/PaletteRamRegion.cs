using System;
using System.Collections.Generic;

namespace AgbSharp.Core.Memory.Ram
{
    class PaletteRamRegion : IMemoryRegion
    {
        public const uint REGION_START = 0x05000000;
        public const uint REGION_SIZE = 0x400;

        private byte[] Data;

        public PaletteRamRegion()
        {
            Data = new byte[REGION_SIZE];
        }

        public IEnumerable<Tuple<uint, uint>> GetHandledRanges()
        {
            return new List<Tuple<uint, uint>>()
            {
                new Tuple<uint, uint>(REGION_START, REGION_SIZE)
            };
        }

        public byte Read(uint address)
        {
            return Data[address - REGION_START];
        }

        public void Write(uint address, byte val)
        {
            Data[address - REGION_START] = val;
        }

    }
}