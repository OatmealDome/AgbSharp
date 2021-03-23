using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_Basic_Tests
    {
        [Fact]
        public void Branch_BranchWithPositiveOffset_PcCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateAndRunCpu(new uint[]
            {
                0xEA0003FE // B #0x1000
            });

            Assert.Equal(InternalWramRegion.REGION_START + 0x1000, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
        }

        [Fact]
        public void Branch_BranchWithNegativeOffset_PcCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateAndRunCpu(new uint[]
            {
                0xEAFFFBFE // B #-0x1000
            });

            Assert.Equal(InternalWramRegion.REGION_START - 0x1000, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
        }

        [Fact]
        public void BranchWithLink_BranchWithPositiveOffset_PcAndLrCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateAndRunCpu(new uint[]
            {
                0xEB0003FE // BL #0x1000
            });

            Assert.Equal(InternalWramRegion.REGION_START + 0x1000, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
            Assert.Equal(InternalWramRegion.REGION_START + 0x4, cpu.CurrentRegisterSet.GetRegister(CpuUtil.LR));
        }

        [Fact]
        public void BranchWithLink_BranchWithNegativeOffset_PcAndLrCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateAndRunCpu(new uint[]
            {
                0xEBFFFBFE // BL #-0x1000
            });

            Assert.Equal(InternalWramRegion.REGION_START - 0x1000, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
            Assert.Equal(InternalWramRegion.REGION_START + 0x4, cpu.CurrentRegisterSet.GetRegister(CpuUtil.LR));
        }

        [Fact]
        public void BranchAndExchange_BranchToRegZero_PcCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE12FFF10 // BX r0
            });

            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
        }

        [Fact]
        public void BranchAndExchange_BranchToRegZeroInThumb_PcCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE12FFF11 // BX r1
            });

            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
            Assert.True(cpu.CurrentStatus.Thumb);
        }

    }
}