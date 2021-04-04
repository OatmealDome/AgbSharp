using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Interrupt;
using AgbSharp.Core.Cpu.Status;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu
{
    public class AgbCpu_InterruptHandling_Tests
    {
        [Fact]
        public void RaiseInterrupt_InterruptsEnabledWithVBlankIrq_InterruptHandled()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.RegisterValue = 0b11111000000000000000000000110000;

            cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC) = InternalWramRegion.REGION_START;

            cpu.MemoryMap.WriteU32(0x4000208, 1); // IME = 1
            cpu.MemoryMap.WriteU16(0x4000200, 1); // IE = VBlank IRQ enabled

            cpu.MemoryMap.FlushMmio();

            cpu.RaiseInterrupt(InterruptType.VBlank);

            cpu.MemoryMap.UpdateMmio();

            Assert.Equal(CpuMode.Irq, cpu.CurrentStatus.Mode);
            Assert.False(cpu.CurrentStatus.Thumb);
            Assert.True(cpu.CurrentStatus.IrqDisable);
            Assert.Equal((uint)0b11111000000000000000000000110000, cpu.CurrentSavedStatus.RegisterValue);
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(CpuUtil.LR));
            Assert.Equal((uint)0x00000018, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
            Assert.Equal(1, cpu.MemoryMap.ReadU16(0x4000202)); // IF = VBlank
        }

        [Fact]
        public void RaiseInterrupt_MasterFlagDisabled_InterruptNotHandled()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(0x4000208, 0); // IME = 0
            cpu.MemoryMap.WriteU16(0x4000200, 1); // IE = VBlank IRQ enabled

            cpu.MemoryMap.FlushMmio();

            cpu.RaiseInterrupt(InterruptType.VBlank);

            Assert.Equal(CpuMode.User, cpu.CurrentStatus.Mode);
        }

        [Fact]
        public void RaiseInterrupt_IndividualEnableFlagDisabled_InterruptNotHandled()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(0x4000208, 1); // IME = 1
            cpu.MemoryMap.WriteU16(0x4000200, 0); // IE = VBlank IRQ disabled

            cpu.MemoryMap.FlushMmio();

            cpu.RaiseInterrupt(InterruptType.VBlank);

            Assert.Equal(CpuMode.User, cpu.CurrentStatus.Mode);
        }

        [Fact]
        public void RaiseInterrupt_CpsrIrqFlagDisabled_InterruptNotHandled()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.IrqDisable = true;

            cpu.MemoryMap.WriteU32(0x4000208, 1); // IME = 1
            cpu.MemoryMap.WriteU16(0x4000200, 1); // IE = VBlank IRQ enabled

            cpu.MemoryMap.FlushMmio();

            cpu.RaiseInterrupt(InterruptType.VBlank);

            Assert.Equal(CpuMode.User, cpu.CurrentStatus.Mode);
        }

    }
}