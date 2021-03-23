using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests
{
    public class ArmInterpreter_Basic_Tests
    {
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

            cpu.CurrentRegisterSet.GetRegister(15) = InternalWramRegion.REGION_START;

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
                0xEA0003FE
            });

            Assert.Equal(InternalWramRegion.REGION_START + 0x1000, cpu.CurrentRegisterSet.GetRegister(15));
        }

        [Fact]
        public void Branch_BranchWithNegativeOffset_PcCorrect()
        {
            AgbCpu cpu = CreateAndRunCpu(new uint[]
            {
                0xEAFFFBFE
            });

            Assert.Equal(InternalWramRegion.REGION_START - 0x1000, cpu.CurrentRegisterSet.GetRegister(15));
        }

    }
}