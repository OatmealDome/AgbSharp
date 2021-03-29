using System.Collections.Generic;
using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormSixteenConditionalBranch_Tests
    {
        [Theory]
        [MemberData(nameof(FlagsAndOpcodes))]
        public void Branch_BranchWithPsotiveOffsetAndCondition_PcCorrect(uint flags, uint opcode)
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.RegisterValue = (cpu.CurrentStatus.RegisterValue & 0x0FFFFFFF) | (flags << 28);

            const uint baseInstruction = 0xD07F;
            uint instruction = (opcode << 8) | baseInstruction;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                (ushort)instruction // B{cond} #0xFE
            }, true);

            Assert.Equal(InternalWramRegion.REGION_START + 0x4 + 0xFE, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
        }

        [Fact]
        public void Branch_BranchWithNegativeOffsetAndPassingCondition_PcCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Zero = true;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xD080 // B{cond} #0xFF
            }, true);

            Assert.Equal(InternalWramRegion.REGION_START + 0x4 - 256, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
        }

        public static IEnumerable<object[]> FlagsAndOpcodes => new List<object[]>
        {
            //                     NZCV
            new object[] { (uint)0b0100, 0x0},
            new object[] { (uint)0b1011, 0x1 },
            new object[] { (uint)0b0010, 0x2 },
            new object[] { (uint)0b1101, 0x3 },
            new object[] { (uint)0b1000, 0x4 },
            new object[] { (uint)0b0111, 0x5 },
            new object[] { (uint)0b0001, 0x6 },
            new object[] { (uint)0b1110, 0x7 },
            new object[] { (uint)0b1011, 0x8 },
            new object[] { (uint)0b1001, 0x9 },
            new object[] { (uint)0b1111, 0x9 },
            new object[] { (uint)0b1001, 0xA },
            new object[] { (uint)0b0110, 0xA },
            new object[] { (uint)0b0111, 0xB},
            new object[] { (uint)0b1110, 0xB},
            new object[] { (uint)0b1011, 0xC},
            new object[] { (uint)0b0010, 0xC},
            new object[] { (uint)0b1101, 0xD},
            new object[] { (uint)0b1010, 0xD},
            new object[] { (uint)0b0011, 0xD},
        };

    }
}