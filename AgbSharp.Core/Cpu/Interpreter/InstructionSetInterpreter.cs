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
        // Addition/Subtraction helpers
        //
        // Thanks to mGBA for easy shortcuts.
        // include/mgba/internal/arm/isa-inlines.h
        //
        // Additional notes on Overflow flag:
        // http://teaching.idallen.com/dat2343/10f/notes/040_overflow.txt
        //

        protected void SetCarryAndOverflowOnAddition(uint first, uint second, uint result)
        {
            CurrentStatus.Carry = (BitUtil.GetBit(first, 31) + BitUtil.GetBit(second, 31)) > BitUtil.GetBit(result, 31);
            CurrentStatus.Overflow = !BitUtil.IsBitSet(first ^ second, 31) && BitUtil.IsBitSet(first ^ result, 31);
        }

        protected void SetCarryAndOverflowOnSubtraction(uint first, uint second, uint result, bool carry)
        {
            if (carry)
            {
                CurrentStatus.Carry = first >= (second + (CurrentStatus.Carry ? 0 : 1));
            }
            else
            {
                CurrentStatus.Carry = first >= second;
            }

            CurrentStatus.Overflow = BitUtil.IsBitSet(first ^ second, 31) && BitUtil.IsBitSet(first ^ result, 31);
        }

        //
        // Multiply helpers
        //

        protected int GetMBasedOnAllOnesOrZerosForMultiply(uint reg)
        {
            if ((reg & 0xFFFFFF00) == 0xFFFFFF00 || (reg & 0xFFFFFF00) == 0x00000000)
            {
                return 1;
            }
            else if ((reg & 0xFFFF0000) == 0xFFFF0000 || (reg & 0xFFFF0000) == 0x00000000)
            {
                return 2;
            }
            else if ((reg & 0xFF000000) == 0xFF000000 || (reg & 0xFF000000) == 0x00000000)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        protected int GetMBasedOnAllZerosForMultiply(uint reg)
        {
            if ((reg & 0xFFFFFF00) == 0x00000000)
            {
                return 1;
            }
            else if ((reg & 0xFFFF0000) == 0x00000000)
            {
                return 2;
            }
            else if ((reg & 0xFF000000) == 0x00000000)
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        //
        // Data Block Transfer (STM / LDM) Helpers
        //

        protected int PerformDataBlockTransfer(ref uint nReg, bool isPreIndex, bool isUp, bool isWriteBack, bool isLoad, bool useUserBank, uint regBitfield)
        {
            int transferredWords = 0;

            for (int i = 0; i < 16; i++)
            {
                int regNum;

                if (isPreIndex && !isUp)
                {
                    regNum = 15 - i;
                }
                else
                {
                    regNum = i;
                }

                if (!BitUtil.IsBitSet(regBitfield, regNum))
                {
                    continue;
                }

                uint address;
                if (isUp)
                {
                    address = nReg + 4;
                }
                else
                {
                    address = nReg - 4;
                }

                uint effectiveAddress;
                if (isPreIndex)
                {
                    effectiveAddress = address;
                }
                else
                {
                    effectiveAddress = nReg;
                }

                if (isLoad)
                {
                    uint value = Cpu.MemoryMap.ReadU32(effectiveAddress);

                    if (useUserBank)
                    {
                        Cpu.RegUser(regNum) = value;
                    }
                    else
                    {
                        Reg(regNum) = value;
                    }
                }
                else
                {
                    if (useUserBank)
                    {
                        Cpu.MemoryMap.WriteU32(effectiveAddress, Cpu.RegUser(regNum));
                    }
                    else
                    {
                        Cpu.MemoryMap.WriteU32(effectiveAddress, Reg(regNum));
                    }
                }

                if (isWriteBack || !isPreIndex)
                {
                    nReg = address;
                }

                transferredWords++;
            }

            return transferredWords;
        }

        //
        // Interpreter must implement these
        //

        public abstract int Step();

    }
}