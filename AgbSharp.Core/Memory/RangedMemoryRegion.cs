using System.Collections.Generic;

namespace AgbSharp.Core.Memory
{
    public abstract class RangedMemoryRegion : IMemoryRegion
    {
        public readonly uint RegionStart;

        public readonly uint RegionSize;

        public readonly uint MirrorEnd;

        protected uint Mask
        {
            get
            {
                return RegionSize - 1;
            }
        }

        private byte[] Data;

        public RangedMemoryRegion(uint start, uint size, uint mirrorEnd, byte[] data = null)
        {
            RegionStart = start;
            RegionSize = size;
            MirrorEnd = mirrorEnd;

            if (data == null)
            {
                Data = new byte[RegionSize];
            }
            else
            {
                Data = data;
            }
        }

        public virtual IEnumerable<byte> GetHandledRanges()
        {
            return new List<byte>()
            {
                (byte)(RegionStart >> 24)
            };
        }

        public bool IsValidAddress(uint address)
        {
            return address >= RegionStart && address <= MirrorEnd;
        }

        public byte Read(uint address)
        {
            return Data[address & Mask];
        }

        public virtual void Write(uint address, byte val)
        {
            Data[address & Mask] = val;
        }

    }
}