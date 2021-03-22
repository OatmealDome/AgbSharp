using System;

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

    }
}