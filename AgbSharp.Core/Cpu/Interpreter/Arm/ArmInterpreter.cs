using System;
using AgbSharp.Core.Util;

namespace AgbSharp.Core.Cpu.Interpreter.Arm
{
    class ArmInterpreter : InstructionSetInterpreter
    {
        public ArmInterpreter(AgbCpu cpu) : base(cpu)
        {
        }

        public override int Step()
        {
            uint instruction = 0; // MemoryRead(Reg(PC));
            Reg(PC) += 4;

            if (!CheckCondition(instruction >> 28))
            {
                return 1; // 1S
            }

            int instructionType = BitUtil.GetBitRange(instruction, 26, 27);
            switch (instructionType)
            {
                case 0b10: // Branch
                    return Branch(instruction);
            }

            InterpreterAssert($"Unknown instruction ({instruction:x8}");

            return 0;
        }

        private bool CheckCondition(uint conditionBits)
        {
            InterpreterAssert(Enum.IsDefined(typeof(ArmInstructionCondition), conditionBits), "Invalid condition bits");

            ArmInstructionCondition condition = (ArmInstructionCondition)conditionBits;

            switch (condition)
            {
                case ArmInstructionCondition.Always:
                    return true;
                case ArmInstructionCondition.Equal:
                    return CurrentStatus.Zero;
                case ArmInstructionCondition.NotEqual:
                    return !CurrentStatus.Zero;
                case ArmInstructionCondition.CarrySet:
                    return CurrentStatus.Carry;
                case ArmInstructionCondition.CarryClear:
                    return !CurrentStatus.Carry;
                case ArmInstructionCondition.Negative:
                    return CurrentStatus.Negative;
                case ArmInstructionCondition.Positive:
                    return !CurrentStatus.Negative;
                case ArmInstructionCondition.Overflow:
                    return CurrentStatus.Overflow;
                case ArmInstructionCondition.OverflowClear:
                    return !CurrentStatus.Overflow;
                case ArmInstructionCondition.UnsignedHigher:
                    return CurrentStatus.Carry && !CurrentStatus.Zero;
                case ArmInstructionCondition.UnsignedLowerOrSame:
                    return !CurrentStatus.Carry && CurrentStatus.Zero;
                case ArmInstructionCondition.GreaterThanOrEqual:
                    return CurrentStatus.Negative == CurrentStatus.Overflow;
                case ArmInstructionCondition.LessThan:
                    return CurrentStatus.Negative != CurrentStatus.Overflow;
                case ArmInstructionCondition.GreaterThan:
                    return !CurrentStatus.Zero && CurrentStatus.Negative == CurrentStatus.Overflow;
                case ArmInstructionCondition.LessThanOrEqual:
                    return CurrentStatus.Zero && CurrentStatus.Negative != CurrentStatus.Overflow;
            }

            // Should never reach here
            InterpreterAssert("Reached unreachable code in CheckCondition");

            return false;
        }

        private int Branch(uint instruction)
        {
            uint offset = (uint)BitUtil.GetBitRange(instruction, 0, 23);

            if (BitUtil.IsBitSet(instruction, 24)) // BL
            {
                Reg(LR) = Reg(PC);
            }

            Reg(PC) += 4;
            Reg(PC) += 4 * offset;

            return 2 + 1; // 2S + 1N
        }

    }
}