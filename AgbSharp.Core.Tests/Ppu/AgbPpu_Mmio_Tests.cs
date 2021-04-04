using AgbSharp.Core.Memory;
using AgbSharp.Core.Ppu;
using Xunit;

namespace AgbSharp.Core.Tests.Ppu
{
    public class AgbPpu_Mmio_Tests
    {
        #region DISPCNT

        [Fact]
        public void Dispcnt_SetAllBitsWithMemoryMap_AllBitsSet()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap);

            memoryMap.WriteU16(0x4000000, 0xFFFF);

            memoryMap.FlushMmio();

            Assert.Equal(0xFFFF, memoryMap.ReadU16(0x4000000));
        }

        #endregion

        #region DISPSTAT

        [Fact]
        public void Dispstat_SetAllConfigurableBitsWithMemoryMap_AllConfigurableBitsSet()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap);

            memoryMap.WriteU16(0x4000004, 0xFF38);

            memoryMap.FlushMmio();

            Assert.Equal(0xFF38, memoryMap.ReadU16(0x4000004));
        }

        [Fact]
        public void Dispstat_TickUntilVBlank_VBlankFlagSet()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap);

            memoryMap.WriteU16(0x4000004, 0xFF00); // VCount = line 255 (impossible)

            memoryMap.FlushMmio();

            PpuUtil.TickPpuByAmount(ppu, 160, 0);

            memoryMap.UpdateMmio();

            Assert.Equal(0xFF01, memoryMap.ReadU16(0x4000004)); // V-Blank flag
        }

        [Fact]
        public void Dispstat_TickUntilHBlank_HBlankFlagSet()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap);

            memoryMap.WriteU16(0x4000004, 0xFF00); // VCount = line 255 (impossible)

            memoryMap.FlushMmio();

            PpuUtil.TickPpuByAmount(ppu, 0, 240);

            memoryMap.UpdateMmio();

            Assert.Equal(0xFF02, memoryMap.ReadU16(0x4000004)); // H-Blank flag
        }

        [Fact]
        public void Dispstat_TickUntilHBlankPeriodInVBlank_HBlankFlagSet()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap);

            memoryMap.WriteU16(0x4000004, 0xFF00); // VCount = line 255 (impossible)

            memoryMap.FlushMmio();

            PpuUtil.TickPpuByAmount(ppu, 160, 240);

            memoryMap.UpdateMmio();

            Assert.Equal(0xFF03, memoryMap.ReadU16(0x4000004)); // H-Blank and V-Blank flags
        }

        [Fact]
        public void Dispstat_SetVCountAndTickUntilVCountLine_VCountFlagSet()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap);

            memoryMap.WriteU16(0x4000004, 0x6400); // VCount = line 100

            memoryMap.FlushMmio();

            PpuUtil.TickPpuByAmount(ppu, 100, 0);

            memoryMap.UpdateMmio();

            Assert.Equal(0x6404, memoryMap.ReadU16(0x4000004)); // V-Count flag
        }

        #endregion

        #region VCOUNT

        [Fact]
        public void Vcount_TickPpuToLine100_VcountCorrect()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap);

            PpuUtil.TickPpuByAmount(ppu, 100, 0);

            memoryMap.UpdateMmio();

            Assert.Equal(0x0064, memoryMap.ReadU16(0x4000006)); // V-Count flag
        }

        #endregion

    }
}