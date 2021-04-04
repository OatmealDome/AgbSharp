using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Status;
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

        [Fact]
        public void Dispstat_EnableVBlankIrqTickUntilVBlank_InterruptRequested()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbCpu cpu = new AgbCpu(memoryMap);

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap, cpu);

            memoryMap.WriteU32(0x4000208, 0x01); // Interrupt Master Enable
            memoryMap.WriteU16(0x4000200, 0x0001); // IE = V-Blank IRQ enabled
            memoryMap.WriteU16(0x4000004, 0x0008); // DISPSTAT = V-Blank IRQ enabled

            memoryMap.FlushMmio();

            PpuUtil.TickPpuByAmount(ppu, 160, 0);

            memoryMap.UpdateMmio();

            Assert.Equal(0x0009, memoryMap.ReadU16(0x4000004)); // V-Blank flag, V-Blank IRQ enabled
            Assert.Equal(0x0001, memoryMap.ReadU16(0x4000202)); // IF = V-Count
            Assert.Equal(CpuMode.Irq, cpu.CurrentStatus.Mode);
        }

        [Fact]
        public void Dispstat_EnableHBlankIrqAndTickUntilHBlank_InterruptRequested()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbCpu cpu = new AgbCpu(memoryMap);

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap, cpu);

            memoryMap.WriteU32(0x4000208, 0x01); // Interrupt Master Enable
            memoryMap.WriteU16(0x4000200, 0x0002); // IE = H-Blank IRQ enabled
            memoryMap.WriteU16(0x4000004, 0xFF10); // DISPSTAT = H-Blank IRQ enabled, VCount = line 255

            memoryMap.FlushMmio();

            PpuUtil.TickPpuByAmount(ppu, 0, 240);

            memoryMap.UpdateMmio();

            Assert.Equal(0xFF12, memoryMap.ReadU16(0x4000004)); // H-Blank flag, H-Blank IRQ enabled
            Assert.Equal(0x0002, memoryMap.ReadU16(0x4000202)); // IF = H-Count
            Assert.Equal(CpuMode.Irq, cpu.CurrentStatus.Mode);
        }

        [Fact]
        public void Dispstat_SetVCountAndEnableVCountIrqAndTickUntilVCountLine_InterruptRequested()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            AgbCpu cpu = new AgbCpu(memoryMap);

            AgbPpu ppu = PpuUtil.CreatePpu(memoryMap, cpu);

            memoryMap.WriteU32(0x4000208, 0x01); // Interrupt Master Enable
            memoryMap.WriteU16(0x4000200, 0x0004); // IE = V-Count IRQ enabled
            memoryMap.WriteU16(0x4000004, 0x6420); // DISPSTAT = V-Blank IRQ enabled, VCount = line 100

            memoryMap.FlushMmio();

            PpuUtil.TickPpuByAmount(ppu, 100, 0);

            memoryMap.UpdateMmio();

            Assert.Equal(0x6424, memoryMap.ReadU16(0x4000004)); // V-Count flag, V-Count IRQ enabled
            Assert.Equal(0x0004, memoryMap.ReadU16(0x4000202)); // IF = V-Count
            Assert.Equal(CpuMode.Irq, cpu.CurrentStatus.Mode);
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