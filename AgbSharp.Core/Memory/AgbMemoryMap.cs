using System;
using System.Collections.Generic;
using AgbSharp.Core.Memory.Mmio;
using AgbSharp.Core.Util;

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
        private readonly List<IMmioRegion> MmioRegions;
        private readonly UniqueQueue<IMmioRegion> DirtyRegions;

        public AgbMemoryMap()
        {
            Map = new Dictionary<uint, IMemoryRegion>();
            MmioRegions = new List<IMmioRegion>();
            DirtyRegions = new UniqueQueue<IMmioRegion>();
        }

        public void RegisterRegion(IMemoryRegion region)
        {
            foreach (Tuple<uint, uint> range in region.GetHandledRanges())
            {
                for (uint i = range.Item1; i < range.Item1 + range.Item2; i++)
                {
                    Map[i] = region;
                }
            }
        }

        public void RegisterMmio(uint address, Func<byte> readFunc, Action<byte> writeFunc)
        {
            MmioByteRegion region = new MmioByteRegion(address, readFunc, writeFunc);

            Map[address] = region;

            MmioRegions.Add(region);
        }

        public void RegisterMmio16(uint address, Func<ushort> readFunc, Action<ushort> writeFunc)
        {
            MmioHalfWordRegion region = new MmioHalfWordRegion(address, readFunc, writeFunc);

            Map[address] = region;
            Map[address + 1] = region;

            MmioRegions.Add(region);
        }

        public void RegisterMmio32(uint address, Func<uint> readFunc, Action<uint> writeFunc)
        {
            MmioWordRegion region = new MmioWordRegion(address, readFunc, writeFunc);

            Map[address] = region;
            Map[address + 1] = region;
            Map[address + 2] = region;
            Map[address + 3] = region;

            MmioRegions.Add(region);
        }

        public byte Read(uint address)
        {
            if (Map.TryGetValue(address, out IMemoryRegion region))
            {
                return region.Read(address);
            }
            else
            {
                throw new Exception($"invalid read addr 0x{address:x8}");
            }
        }

        public void Write(uint address, byte val)
        {
            if (Map.TryGetValue(address, out IMemoryRegion region))
            {
                IMmioRegion mmioRegion = region as IMmioRegion;
                if (mmioRegion != null)
                {
                    DirtyRegions.Enqueue(mmioRegion);
                }

                region.Write(address, val);
            }
            else
            {
                throw new Exception($"invalid write addr 0x{address:x8}");
            }
        }

        public uint ReadU32(uint address)
        {
            return ((uint)Read(address + 3) << 24) | ((uint)Read(address + 2) << 16) | ((uint)Read(address + 1) << 8) | Read(address);
        }

        public void WriteU32(uint address, uint val)
        {
            Write(address + 3, (byte)(val >> 24));
            Write(address + 2, (byte)((val >> 16) & 0xFF));
            Write(address + 1, (byte)((val >> 8) & 0xFF));
            Write(address, (byte)(val & 0xFF));
        }

        public ushort ReadU16(uint address)
        {
            return (ushort)((ushort)(Read(address + 1) << 8) | Read(address));
        }

        public void WriteU16(uint address, ushort val)
        {
            Write(address + 1, (byte)((val >> 8) & 0xFF));
            Write(address, (byte)(val & 0xFF));
        }

        public void UpdateMmio()
        {
            foreach (IMmioRegion region in MmioRegions)
            {
                region.Update();
            }
        }

        public void FlushMmio()
        {
            while (DirtyRegions.TryDequeue(out IMmioRegion region))
            {
                region.Flush();
            }
        }
        
    }
}