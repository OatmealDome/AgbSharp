using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests
{
    public class ArmInterpreter_Basic_Tests
    {
        private const int SP = 13;
        private const int LR = 14;
        private const int PC = 15;

        private static AgbCpu CreateCpu()
        {
            AgbMemoryMap map = new AgbMemoryMap();

            map.RegisterRegion(new InternalWramRegion());

            AgbCpu cpu = new AgbCpu(map);

            return cpu;
        }

        private static void RunCpu(AgbCpu cpu, uint[] instructions)
        {
            for (int i = 0; i < instructions.Length; i++)
            {
                cpu.MemoryMap.WriteU32(InternalWramRegion.REGION_START + (uint)i * 4, instructions[i]);
            }

            cpu.CurrentRegisterSet.GetRegister(PC) = InternalWramRegion.REGION_START;

            for (int i = 0; i < instructions.Length; i++)
            {
                cpu.Step();
            }
        }

        private static AgbCpu CreateAndRunCpu(uint[] instructions)
        {
            AgbCpu cpu = CreateCpu();

            RunCpu(cpu, instructions);

            return cpu;
        }

        [Fact]
        public void Branch_BranchWithPositiveOffset_PcCorrect()
        {
            AgbCpu cpu = CreateAndRunCpu(new uint[]
            {
                0xEA0003FE // B #0x1000
            });

            Assert.Equal(InternalWramRegion.REGION_START + 0x1000, cpu.CurrentRegisterSet.GetRegister(PC));
        }

        [Fact]
        public void Branch_BranchWithNegativeOffset_PcCorrect()
        {
            AgbCpu cpu = CreateAndRunCpu(new uint[]
            {
                0xEAFFFBFE // B #-0x1000
            });

            Assert.Equal(InternalWramRegion.REGION_START - 0x1000, cpu.CurrentRegisterSet.GetRegister(PC));
        }

        [Fact]
        public void BranchWithLink_BranchWithPositiveOffset_PcAndLrCorrect()
        {
            AgbCpu cpu = CreateAndRunCpu(new uint[]
            {
                0xEB0003FE // BL #0x1000
            });

            Assert.Equal(InternalWramRegion.REGION_START + 0x1000, cpu.CurrentRegisterSet.GetRegister(PC));
            Assert.Equal(InternalWramRegion.REGION_START + 0x4, cpu.CurrentRegisterSet.GetRegister(LR));
        }

        [Fact]
        public void BranchWithLink_BranchWithNegativeOffset_PcAndLrCorrect()
        {
            AgbCpu cpu = CreateAndRunCpu(new uint[]
            {
                0xEBFFFBFE // BL #-0x1000
            });

            Assert.Equal(InternalWramRegion.REGION_START - 0x1000, cpu.CurrentRegisterSet.GetRegister(PC));
            Assert.Equal(InternalWramRegion.REGION_START + 0x4, cpu.CurrentRegisterSet.GetRegister(LR));
        }

        [Fact]
        public void BranchAndExchange_BranchToRegZero_PcCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            RunCpu(cpu, new uint[]
            {
                0xE12FFF10 // BX r0
            });

            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(PC));
        }

        [Fact]
        public void BranchAndExchange_BranchToRegZeroInThumb_PcCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            RunCpu(cpu, new uint[]
            {
                0xE12FFF11 // BX r1
            });

            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(PC));
            Assert.True(cpu.CurrentStatus.Thumb);
        }

    }
}