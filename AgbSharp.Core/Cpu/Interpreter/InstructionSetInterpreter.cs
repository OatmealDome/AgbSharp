using AgbSharp.Core.Cpu.Status;
using AgbSharp.Core.Util;

namespace AgbSharp.Core.Cpu.Interpreter
{
    abstract class InstructionSetInterpreter
    {
        protected AgbCpu Cpu;

        // Easy accessors for properties in AgbCpu
        protected ProgramStatus CurrentStatus
        {
            get
            {
                return Cpu.CurrentStatus;
            }
        }

        protected ProgramStatus SavedStatus
        {
            get
            {
                return Cpu.CurrentSavedStatus;
            }
        }

        // Constants to use with Reg()
        protected const int SP = 13;
        protected const int LR = 14;
        protected const int PC = 15;

        protected InstructionSetInterpreter(AgbCpu cpu)
        {
            Cpu = cpu;
        }

        //
        // Helper functions
        //

        protected ref uint Reg(int reg)
        {
            return ref Cpu.CurrentRegisterSet.GetRegister(reg);
        }

        protected void InterpreterAssert(string message)
        {
            throw new CpuException(message);
        }

        protected void InterpreterAssert(bool expression, string message)
        {
            if (!expression)
            {
                throw new CpuException(message);
            }
        }

        //
        // Shift
        //

        protected uint PerformShift(ShiftType shiftType, uint operand, int shift, bool isZeroSpecialCase, bool setConditionCodes)
        {
            switch (shiftType)
            {
                case ShiftType.LogicalLeft: // LSL
                    if (!isZeroSpecialCase)
                    {
                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, 32 - shift);
                        }

                        operand <<= shift;
                    }

                    break;
                case ShiftType.LogicalRight: // LSR
                    if (isZeroSpecialCase)
                    {
                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, 31);
                        }

                        operand = 0;
                    }
                    else
                    {
                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, shift - 1);
                        }

                        operand >>= shift;
                    }

                    break;
                case ShiftType.ArithmaticRight: // ASR
                    if (isZeroSpecialCase)
                    {
                        if (BitUtil.IsBitSet(operand, 31))
                        {
                            operand = 0xFFFFFFFF;
                        }
                        else
                        {
                            operand = 0x00000000;
                        }
                    }
                    else
                    {
                        // C# will do an ASR if the left operand is an int
                        operand = (uint)((int)operand >> shift);
                    }

                    if (setConditionCodes)
                    {
                        CurrentStatus.Carry = BitUtil.IsBitSet(operand, 31);
                    }

                    break;
                case ShiftType.RotateRight: // ROR
                    if (isZeroSpecialCase)
                    {
                        operand = BitUtil.RotateRight(operand, 1);

                        if (CurrentStatus.Carry)
                        {
                            BitUtil.SetBit(ref operand, 31);
                        }
                        else
                        {
                            BitUtil.ClearBit(ref operand, 31);
                        }
                    }
                    else
                    {
                        operand = BitUtil.RotateRight(operand, shift);

                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, shift - 1);
                        }
                    }

                    break;
            }

            return operand;
        }


        //
        // Interpreter must implement these
        //

        public abstract int Step();

    }
}