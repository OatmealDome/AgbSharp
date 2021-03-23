using System;
using System.Collections.Generic;

namespace AgbSharp.Core.Memory
{
    //
    // AGB memory map:
    //
    // 0x00000000 to 0x00003FFF - AGB IPL/BIOS
    // 0x02000000 to 0x0203FFFF - 256kb External WRAM (16-bit)
    // 0x03000000 to 0x03007FFF - 32kb Internal WRAM (32-bit)
    // 0x04000000 to 0x040003FF - MMIO
    // 0x05000000 to 0x050003FF - Palette RAM
    // 0x06000000 to 0x06017FFF - VRAM
    // 0x07000000 to 0x070003FF - OAM
    // 0x08000000 to .......... - Game Pak ROM (16-bit)
    // 0x0E000000 to .......... - Game Pak RAM (8-bit)
    //

    class AgbMemoryMap
    {
        private readonly Dictionary<uint, IMemoryRegion> Map;

        public AgbMemoryMap()
        {
            Map = new Dictionary<uint, IMemoryRegion>();
        }

        public void RegisterRegion(IMemoryRegion region)
        {
            foreach (Tuple<uint, uint> range in region.GetHandledRanges())
            {
                for (uint i = range.Item1; i < range.Item2; i++)
                {
                    Map[i] = region;
                }
            }
        }

        public void RegisterMmio(uint address, Func<byte> readFunc, Action<byte> writeFunc)
        {
            Map[address] = new MmioRegion(readFunc, writeFunc);
        }

        public byte Read(uint address)
        {
            if (Map.TryGetValue(address, out IMemoryRegion region))
            {
                return region.Read(address);
            }
            else
            {
                throw new Exception($"invalid read addr {address}");
            }
        }

        public void Write(uint address, byte val)
        {
            if (Map.TryGetValue(address, out IMemoryRegion region))
            {
                region.Write(address, val);
            }
            else
            {
                throw new Exception($"invalid write addr {address}");
            }
        }
        
    }
}