using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_Swap_Tests
    {
        [Fact]
        public void Swap_WriteRegTwoToRegZeroAndWriteRegOneToRegTwo_SwapSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xDEADBEEF);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFEEDC0DE;
            cpu.CurrentRegisterSet.GetRegister(2) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x910002E1 // SWP r0, r1, [r2]
            }, true);

            Assert.Equal(0xFEEDC0DE, cpu.MemoryMap.ReadU32(targetAddress));
            Assert.Equal(0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(0xFEEDC0DE, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void SwapByte_WriteRegTwoToRegZeroAndWriteRegOneToRegTwo_SwapSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xFF);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x000000AA;
            cpu.CurrentRegisterSet.GetRegister(2) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x910002E1 // SWP r0, r1, [r2]
            }, true);

            Assert.Equal((uint)0x000000AA, cpu.MemoryMap.ReadU32(targetAddress));
            Assert.Equal((uint)0x000000FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x000000AA, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(2));
        }

    }
}