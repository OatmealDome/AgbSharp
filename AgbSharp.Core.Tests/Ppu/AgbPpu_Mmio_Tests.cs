using AgbSharp.Core.Memory;
using AgbSharp.Core.Ppu;
using Xunit;

namespace AgbSharp.Core.Tests.Ppu
{
    public class AgbPpu_Mmio_Tests
    {
        [Fact]
        public void Dispcnt_SetAllBitsWithMemoryMap_AllBitsSet()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap);

            memoryMap.WriteU16(0x4000000, 0xFFFF);

            memoryMap.FlushMmio();

            Assert.Equal(0xFFFF, memoryMap.ReadU16(0x4000000));
        }

    }
}