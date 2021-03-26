using System.Collections.Generic;
using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Interpreter.Arm;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_Condition_Tests
    {
        [Theory]
        [MemberData(nameof(FlagsAndConditions))]
        public void Condition_MoveInstructionUsingCondition_ConditionPasses(uint flags, ArmInstructionCondition condition)
        {
            const uint movInstruction = 0xE1A00001;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.RegisterValue = (cpu.CurrentStatus.RegisterValue & 0x0FFFFFFF) | (flags << 28);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xDEADBEEF;

            uint instruction = movInstruction & 0x0FFFFFFF;
            instruction |= (uint)condition << 28;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                instruction // MOV{cond} r0, r1
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(1));
        }

        public static IEnumerable<object[]> FlagsAndConditions => new List<object[]>
        {
            //                     NZCV
            new object[] { (uint)0b0100, ArmInstructionCondition.Equal },
            new object[] { (uint)0b1011, ArmInstructionCondition.NotEqual },
            new object[] { (uint)0b0010, ArmInstructionCondition.CarrySet },
            new object[] { (uint)0b1101, ArmInstructionCondition.CarryClear },
            new object[] { (uint)0b1000, ArmInstructionCondition.Negative },
            new object[] { (uint)0b0111, ArmInstructionCondition.Positive },
            new object[] { (uint)0b0001, ArmInstructionCondition.Overflow },
            new object[] { (uint)0b1110, ArmInstructionCondition.OverflowClear },
            new object[] { (uint)0b1011, ArmInstructionCondition.UnsignedHigher },
            new object[] { (uint)0b1001, ArmInstructionCondition.UnsignedLowerOrSame },
            new object[] { (uint)0b1111, ArmInstructionCondition.UnsignedLowerOrSame },
            new object[] { (uint)0b1001, ArmInstructionCondition.GreaterThanOrEqual },
            new object[] { (uint)0b0110, ArmInstructionCondition.GreaterThanOrEqual },
            new object[] { (uint)0b0111, ArmInstructionCondition.LessThan },
            new object[] { (uint)0b1110, ArmInstructionCondition.LessThan },
            new object[] { (uint)0b1011, ArmInstructionCondition.GreaterThan },
            new object[] { (uint)0b0010, ArmInstructionCondition.GreaterThan },
            new object[] { (uint)0b1101, ArmInstructionCondition.LessThanOrEqual },
            new object[] { (uint)0b1010, ArmInstructionCondition.LessThanOrEqual },
            new object[] { (uint)0b0011, ArmInstructionCondition.LessThanOrEqual },
            new object[] { (uint)0b0000, ArmInstructionCondition.Always },
            new object[] { (uint)0b0000, ArmInstructionCondition.Reserved }
        };

    }
}