using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormEighteenUnconditionalBranch_Tests
    {
        [Fact]
        public void Branch_BranchWithPositiveOffset_PcCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xE3FF // B #2046
            }, true);

            Assert.Equal(InternalWramRegion.REGION_START + 0x4 + 2046, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
        }

        [Fact]
        public void Branch_BranchWithNegativeOffset_PcCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xE400 // B #-2048
            }, true);

            Assert.Equal(InternalWramRegion.REGION_START + 0x4 - 2048, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
        }

    }
}