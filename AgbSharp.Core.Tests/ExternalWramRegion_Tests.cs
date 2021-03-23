using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests
{
    public class ExternalWramRegion_Tests
    {
        [Fact]
        public void Read_RegisteredRegion_ReadRangeSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            IMemoryRegion region = new ExternalWramRegion();

            // Write direct to the region instead of through AgbMemoryMap
            for (uint i = ExternalWramRegion.REGION_START; i < ExternalWramRegion.REGION_START + ExternalWramRegion.REGION_SIZE; i++)
            {
                region.Write(i, (byte)(i & 0xFF));
            }

            map.RegisterRegion(region);

            for (uint i = ExternalWramRegion.REGION_START; i < ExternalWramRegion.REGION_START + ExternalWramRegion.REGION_SIZE; i++)
            {
                Assert.Equal(i & 0xFF, map.Read(i));
            }
        }

        [Fact]
        public void Write_RegisteredRegion_WriteRangeSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            IMemoryRegion region = new ExternalWramRegion();

            map.RegisterRegion(region);

            for (uint i = ExternalWramRegion.REGION_START; i < ExternalWramRegion.REGION_START + ExternalWramRegion.REGION_SIZE; i++)
            {
                byte value = (byte)(i & 0xFF);

                map.Write(i, value);

                Assert.Equal(value, map.Read(i));
            }
        }

    }
}