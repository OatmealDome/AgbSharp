using System;
using System.Collections.Generic;

namespace AgbSharp.Core.Memory.Ram
{
    class VideoRamRegion : IMemoryRegion
    {
        public const uint REGION_START = 0x06000000;
        public const uint REGION_SIZE = 0x18000;

        private byte[] Data;

        public VideoRamRegion()
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