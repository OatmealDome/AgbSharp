using System;
using AgbSharp.Core.Cpu.Status;
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
            Reg(PC) += 4;
            
            uint instruction = ByteUtil.SwapEndianness(Cpu.MemoryMap.ReadU32(Reg(PC)));

            int cycles = ExecuteInstruction(instruction);

            return cycles;
        }

        private int ExecuteInstruction(uint instruction)
        {
            if (!CheckCondition(instruction >> 28))
            {
                return 1; // 1S
            }

            int instructionType = BitUtil.GetBitRange(instruction, 26, 27);
            switch (instructionType)
            {
                case 0b00:
                    if (BitUtil.GetBitRange(instruction, 8, 25) == 0b010010111111111111) // Branch Exchange
                    {
                        return BranchExchange(instruction);
                    }
                    else if (BitUtil.GetBitRange(instruction, 25, 27) == 0b000 
                            && BitUtil.IsBitSet(instruction, 7)
                            && BitUtil.IsBitSet(instruction, 4)) // Multiply
                    {
                        return MultiplyOperation(instruction);
                    }
                    else if (BitUtil.GetBitRange(instruction, 23, 24) == 0b10
                            && !BitUtil.IsBitSet(instruction, 20))
                    {
                        return PsrOperation(instruction);
                    }
                    else // Data Processing (ALU)
                    {
                        return AluOperation(instruction);
                    }
                case 0b10: // Branch
                    return Branch(instruction);
            }

            InterpreterAssert($"Unknown instruction ({instruction:x8}");

            return 0;
        }

        private bool CheckCondition(uint conditionBits)
        {
            InterpreterAssert(Enum.IsDefined(typeof(ArmInstructionCondition), (int)conditionBits), "Invalid condition bits");

            ArmInstructionCondition condition = (ArmInstructionCondition)conditionBits;

            switch (condition)
            {
                case ArmInstructionCondition.Always:
                case ArmInstructionCondition.Reserved:
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
            int offset = BitUtil.GetBitRange(instruction, 0, 23);

            if (BitUtil.IsBitSet((uint)offset, 23)) // check sign
            {
                // Sign extend
                offset |= ~0x00FFFFFF;
            }

            if (BitUtil.IsBitSet(instruction, 24)) // BL
            {
                Reg(LR) = Reg(PC) + 4;
            }

            Reg(PC) += 8;
            Reg(PC) += (uint)(4 * offset);

            return 2 + 1; // 2S + 1N
        }

        private int BranchExchange(uint instruction)
        {
            int targetRegister = BitUtil.GetBitRange(instruction, 0, 3);

            if (BitUtil.IsBitSet(instruction, 5)) // BLX
            {
                Reg(LR) = Reg(PC) + 4;
            }

            if (BitUtil.IsBitSet(instruction, 0)) // switch to Thumb
            {
                Reg(PC) = Reg(targetRegister - 1);

                CurrentStatus.Thumb = true;
            }
            else // continue in ARM
            {
                Reg(PC) = Reg(targetRegister);
            }

            return 2 + 1; // 2S + 1N
        }

        private uint GetOperandByShiftingRegister(uint instruction, bool setConditionCodes)
        {
            int operandRegNum = BitUtil.GetBitRange(instruction, 0, 3);
            uint operand = Reg(operandRegNum);

            bool isZeroSpecialCase = false;

            int shift;
            if (BitUtil.IsBitSet(instruction, 4))
            {
                if (operandRegNum == PC)
                {
                    operand += 12;
                }

                shift = (int)(Reg(BitUtil.GetBitRange(instruction, 8, 11)) & 0xFF);
            }
            else // immediate
            {
                if (operandRegNum == PC)
                {
                    operand += 8;
                }

                shift = BitUtil.GetBitRange(instruction, 7, 11);

                if (shift == 0)
                {
                    isZeroSpecialCase = true;
                }
            }

            int shiftType = BitUtil.GetBitRange(instruction, 5, 6);
            switch (shiftType)
            {
                case 0b00: // LSL
                    if (!isZeroSpecialCase)
                    {
                        if (setConditionCodes)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(operand, 32 - shift);
                        }

                        operand <<= shift;
                    }

                    break;
                case 0b01: // LSR
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
                case 0b10: // ASR
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
                case 0b11: // ROR
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

        private uint GetSecondOperandForAluOperation(uint instruction, bool setConditionCodes)
        {
            uint secondOperand;

            if (BitUtil.IsBitSet(instruction, 25)) // second operand is immediate
            {
                secondOperand = (uint)BitUtil.GetBitRange(instruction, 0, 7);

                // ROR
                secondOperand = BitUtil.RotateRight(secondOperand, BitUtil.GetBitRange(instruction, 8, 11) * 2);
            }
            else // second operand is register
            {
                secondOperand = GetOperandByShiftingRegister(instruction, setConditionCodes);
            }

            return secondOperand;
        }

        private int AluOperation(uint instruction)
        {
            // for cycle calculation (see GBATEK)
            int p = 0;
            int r = 0;

            bool setConditionCodes = BitUtil.IsBitSet(instruction, 20);

            ref uint firstOperand = ref Reg(BitUtil.GetBitRange(instruction, 16, 19));

            int destinationRegNum = BitUtil.GetBitRange(instruction, 12, 15);
            ref uint destinationReg = ref Reg(destinationRegNum);

            if (destinationRegNum == PC)
            {
                p = 1;
            }

            // Op2 is register and shift by register
            if (BitUtil.IsBitSet(instruction, 25) && BitUtil.IsBitSet(instruction, 4))
            {
                r = 1;
            }

            uint secondOperand = GetSecondOperandForAluOperation(instruction, setConditionCodes);

            uint result;
            
            //
            // Thanks to mGBA for easy shortcuts.
            // include/mgba/internal/arm/isa-inlines.h
            //
            // Additional notes on Overflow flag:
            // http://teaching.idallen.com/dat2343/10f/notes/040_overflow.txt
            //

            void SetCarryAndOverflowOnAddition(uint first, uint second)
            {
                if (!setConditionCodes)
                {
                    return;
                }

                CurrentStatus.Carry = (BitUtil.GetBit(first, 31) + BitUtil.GetBit(second, 31)) > BitUtil.GetBit(result, 31);
                CurrentStatus.Overflow = !BitUtil.IsBitSet(first ^ second, 31) && BitUtil.IsBitSet(first ^ result, 31);
            }

            void SetCarryAndOverflowOnSubtraction(uint first, uint second, bool carry)
            {
                if (!setConditionCodes)
                {
                    return;
                }

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


            int opcode = BitUtil.GetBitRange(instruction, 21, 24);
            switch (opcode)
            {
                case 0b0000: // AND
                case 0b1000: // TST
                    result = firstOperand & secondOperand;
                    break;
                case 0b0001: // EOR (XOR)
                case 0b1001: // TEQ
                    result = firstOperand ^ secondOperand;
                    break;
                case 0b0010: // SUB
                case 0b1010: // CMP
                    result = firstOperand - secondOperand;

                    SetCarryAndOverflowOnSubtraction(firstOperand, secondOperand, false);

                    break;
                case 0b0011: // RSB
                    result = secondOperand - firstOperand;

                    SetCarryAndOverflowOnSubtraction(secondOperand, firstOperand, false);

                    break;
                case 0b0100: // ADD
                case 0b1011: // CMN
                    result = firstOperand + secondOperand;

                    SetCarryAndOverflowOnAddition(firstOperand, secondOperand);

                    break;
                case 0b0101: // ADC
                    result = firstOperand + secondOperand + (uint)(CurrentStatus.Carry ? 1 : 0);

                    SetCarryAndOverflowOnAddition(firstOperand, secondOperand);

                    break;
                case 0b0110: // SBC
                    result = firstOperand - secondOperand + (uint)(CurrentStatus.Carry ? 1 : 0) - 1;
                    
                    SetCarryAndOverflowOnSubtraction(firstOperand, secondOperand, true);

                    break;
                case 0b0111: // RSC
                    result = secondOperand - firstOperand + (uint)(CurrentStatus.Carry ? 1 : 0) - 1;

                    SetCarryAndOverflowOnSubtraction(secondOperand, firstOperand, true);

                    break;
                case 0b1100: // ORR
                    result = firstOperand | secondOperand;
                    break;
                case 0b1101: // MOV
                    result = secondOperand;
                    break;
                case 0b1110: // BIC
                    result = firstOperand & ~secondOperand;
                    break;
                case 0b1111: // MVN
                    result = ~secondOperand;
                    break;
                default:
                    InterpreterAssert($"Invalid ALU opcode {opcode:x}");

                    result = 0;

                    break;
            }

            if (setConditionCodes)
            {
                CurrentStatus.Zero = result == 0;
                CurrentStatus.Negative = BitUtil.IsBitSet(result, 31);

                if (destinationRegNum == PC)
                {
                    InterpreterAssert(CurrentStatus.Mode != CpuMode.User, "ALU instruction with Rd = PC, S = 1 in user mode not allowed");

                    CurrentStatus.RegisterValue = SavedStatus.RegisterValue;
                }
            }

            // TST, TEQ, CMP, and CMN don't write their results to the destination register
            if (opcode != 0b1000 && opcode != 0b1001 && opcode != 0b1010 && opcode != 0b1011)
            {
                destinationReg = result;
            }

            return (1 + p) + r + p; // (1 + p)S + rI + pN
        }
        
        private int MultiplyOperation(uint instruction)
        {
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 16, 19)); // RdHi
            ref uint nReg = ref Reg(BitUtil.GetBitRange(instruction, 12, 15)); // RdLo
            ref uint sReg = ref Reg(BitUtil.GetBitRange(instruction, 8, 11));
            ref uint mReg = ref Reg(BitUtil.GetBitRange(instruction, 0, 3));

            bool setConditionCodes = BitUtil.IsBitSet(instruction, 20);

            bool isLong = false;
            
            int opcode = BitUtil.GetBitRange(instruction, 21, 24);
            switch (opcode)
            {
                case 0b0000: // MUL
                    dReg = mReg * sReg;
                    break;
                case 0b0001: // MLA
                    dReg = (mReg * sReg) + nReg;
                    break;
                case 0b0100: // UMULL
                    ulong umullResult = (ulong)mReg * sReg;

                    dReg = (uint)(umullResult >> 32);
                    nReg = (uint)(umullResult & 0xFFFFFFFF);

                    isLong = true;

                    break;
                case 0b0101: // UMLAL
                    ulong umlalAccumulate = ((ulong)dReg << 32) | nReg;

                    ulong umlalResult = ((ulong)mReg * sReg) + umlalAccumulate;

                    dReg = (uint)(umlalResult >> 32);
                    nReg = (uint)(umlalResult & 0xFFFFFFFF);

                    isLong = true;

                    break;
                case 0b0110: // SMULL
                    int mRegInt = (int)mReg;
                    int sRegInt = (int)sReg;
                    long smullResult = (long)mRegInt * (long)sRegInt;

                    dReg = (uint)(smullResult >> 32);
                    nReg = (uint)(smullResult & 0xFFFFFFFF);

                    isLong = true;

                    break;
                case 0b0111: // SMLAL
                    long smlalAccumulate = (long)(((ulong)dReg << 32) | nReg);

                    int mRegIntSmlal = (int)mReg;
                    int sRegIntSmlal = (int)sReg;
                    long smlalResult = ((long)mRegIntSmlal * sRegIntSmlal) + smlalAccumulate;

                    dReg = (uint)(smlalResult >> 32);
                    nReg = (uint)(smlalResult & 0xFFFFFFFF);

                    isLong = true;

                    break; 
            }

            if (setConditionCodes)
            {
                if (isLong)
                {
                    CurrentStatus.Zero = dReg == 0;
                }
                else
                {
                    CurrentStatus.Zero = dReg == 0 && nReg == 0;
                }
                
                CurrentStatus.Negative = BitUtil.IsBitSet(dReg, 31);
            }

            return 0;
        }

        private int PsrOperation(uint instruction)
        {
            ProgramStatus programStatus;

            if (BitUtil.IsBitSet(instruction, 22))
            {
                programStatus = SavedStatus;   
            }
            else
            {
                programStatus = CurrentStatus;
            }

            if (BitUtil.IsBitSet(instruction, 21)) // MSR
            {
                uint secondOperand = GetSecondOperandForAluOperation(instruction, false);

                uint toWrite = 0;

                if (BitUtil.IsBitSet(instruction, 19))
                {
                    toWrite |= secondOperand & 0xFF000000;
                }
                
                if (BitUtil.IsBitSet(instruction, 18))
                {
                    toWrite |= secondOperand & 0x00FF0000;
                }

                if (BitUtil.IsBitSet(instruction, 17))
                {
                    toWrite |= secondOperand & 0x0000FF00;
                }

                if (BitUtil.IsBitSet(instruction, 16))
                {
                    toWrite |= secondOperand & 0x000000FF;
                }

                programStatus.RegisterValue = toWrite;
            }
            else // MRS
            {
                ref uint destinationReg = ref Reg(BitUtil.GetBitRange(instruction, 12, 15));

                destinationReg = programStatus.RegisterValue;
            }

            return 1; // 1S
        }

    }
}