using System;
using System.Collections.Generic;

namespace AgbSharp.Core.Memory.Mmio
{
    internal abstract class MmioRegion<T> : IMemoryRegion, IMmioRegion
    {
        protected readonly uint BaseAddress;

        protected readonly Func<T> ReadFunc;
        protected readonly Action<T> WriteFunc;

        protected T LastValue;

        public MmioRegion(uint baseAddress, Func<T> readFunc, Action<T> writeFunc)
        {
            BaseAddress = baseAddress;
            ReadFunc = readFunc;
            WriteFunc = writeFunc;

            Update();
        }

        public IEnumerable<Tuple<uint, uint>> GetHandledRanges()
        {
            throw new InvalidOperationException("MMIO region cannot be manually registered");
        }

        //
        // For AgbMemoryMap
        //

        public void Update()
        {
            LastValue = ReadFunc();
        }

        public void Flush()
        {
            WriteFunc(LastValue);
        }

        //
        // Helpers for Read/Write
        //

        protected int BitShiftForAddress(uint address)
        {
            return (int)((address - BaseAddress) * 8);
        }

        //
        // Must implement
        //

        public abstract byte Read(uint address);

        public abstract void Write(uint address, byte val);

        //
        // For HashSet
        //

        public override int GetHashCode()
        {
            return BaseAddress.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            MmioRegion<T> region = obj as MmioRegion<T>;
            if (region == null)
            {
                return false;
            }

            return region.BaseAddress == BaseAddress;
        }

    }
}
