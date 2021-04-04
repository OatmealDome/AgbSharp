using AgbSharp.Core.Memory;
using Xunit;

namespace AgbSharp.Core.Tests.Memory.Ram
{
    public abstract class RamRegion_Tests
    {
        protected abstract uint RegionStart
        {
            get;
        }

        protected abstract uint RegionSize
        {
            get;
        }

        protected abstract IMemoryRegion CreateRegion();

        [Fact]
        public void Read_RegisteredRegion_ReadRangeSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            IMemoryRegion region = CreateRegion();

            // Write direct to the region instead of through AgbMemoryMap
            for (uint i = RegionStart; i < RegionStart + RegionSize; i++)
            {
                region.Write(i, (byte)(i & 0xFF));
            }

            map.RegisterRegion(region);

            for (uint i = RegionStart; i < RegionStart + RegionSize; i++)
            {
                Assert.Equal(i & 0xFF, map.Read(i));
            }
        }

        [Fact]
        public void Write_RegisteredRegion_WriteRangeSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            IMemoryRegion region = CreateRegion();

            map.RegisterRegion(region);

            for (uint i = RegionStart; i < RegionStart + RegionSize; i++)
            {
                byte value = (byte)(i & 0xFF);

                map.Write(i, value);

                Assert.Equal(value, map.Read(i));
            }
        }
        
    }
}