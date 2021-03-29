using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormNineteenLongBranch_Tests
    {
        [Fact]
        public void Branch_LoadHiBits_LrCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xF3FF
            }, true);

            Assert.Equal(cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC) + 2 + (0b01111111111 << 12), cpu.CurrentRegisterSet.GetRegister(CpuUtil.LR));
        }

        [Fact]
        public void Branch_BranchWithNoLoadedBits_PcCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(CpuUtil.LR) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xF800
            }, true);

            Assert.Equal(InternalWramRegion.REGION_START + 0x2, cpu.CurrentRegisterSet.GetRegister(CpuUtil.LR));
            Assert.Equal(0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
        }

        [Fact]
        public void Branch_BranchWithLoadedHiBits_PcCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(CpuUtil.LR) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xF3FF,
                0xFBFF
            }, true);

            Assert.Equal(InternalWramRegion.REGION_START + 0x4, cpu.CurrentRegisterSet.GetRegister(CpuUtil.LR));
            Assert.Equal((uint)0x33FF802, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC)); // TODO: is this correct?
        }

    }
}