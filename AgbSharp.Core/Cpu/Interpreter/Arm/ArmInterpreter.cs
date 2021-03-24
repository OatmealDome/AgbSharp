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
            uint instruction = Cpu.MemoryMap.ReadU32(Reg(PC));
            Reg(PC) += 4;

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
                Reg(LR) = Reg(PC);
            }

            Reg(PC) += 4;
            Reg(PC) += (uint)(4 * offset);

            return 2 + 1; // 2S + 1N
        }

        private int BranchExchange(uint instruction)
        {
            int targetRegister = BitUtil.GetBitRange(instruction, 0, 3);

            if (BitUtil.IsBitSet(instruction, 5)) // BLX
            {
                Reg(LR) = Reg(PC);
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

            uint secondOperand;
            if (BitUtil.IsBitSet(instruction, 25)) // second operand is immediate
            {
                secondOperand = (uint)BitUtil.GetBitRange(instruction, 0, 7);

                // ROR
                secondOperand = BitUtil.RotateRight(secondOperand, BitUtil.GetBitRange(instruction, 8, 11) * 2);
            }
            else // second operand is register
            {
                int secondOperandRegNum = BitUtil.GetBitRange(instruction, 0, 3);
                secondOperand = Reg(secondOperandRegNum);

                bool isZeroSpecialCase = false;

                int shift;
                if (BitUtil.IsBitSet(instruction, 4))
                {
                    if (secondOperandRegNum == PC)
                    {
                        secondOperand += 8;
                    }

                    shift = (int)(Reg(BitUtil.GetBitRange(instruction, 8, 11)) & 0xFF);

                    r = 1;
                }
                else // immediate
                {
                    if (secondOperandRegNum == PC)
                    {
                        secondOperand += 4;
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
                            CurrentStatus.Carry = BitUtil.IsBitSet(secondOperand, 32 - shift);

                            secondOperand <<= shift;
                        }

                        break;
                    case 0b01: // LSR
                        if (isZeroSpecialCase)
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(secondOperand, 31);

                            secondOperand = 0;
                        }
                        else
                        {
                            CurrentStatus.Carry = BitUtil.IsBitSet(secondOperand, shift - 1);

                            secondOperand >>= shift;
                        }

                        break;
                    case 0b10: // ASR
                        if (isZeroSpecialCase)
                        {
                            if (BitUtil.IsBitSet(secondOperand, 31))
                            {
                                secondOperand = 0xFFFFFFFF;
                            }
                            else
                            {
                                secondOperand = 0x00000000;
                            }
                        }
                        else
                        {
                            // C# will do an ASR if the left operand is an int
                            secondOperand = (uint)((int)secondOperand >> shift);
                        }

                        CurrentStatus.Carry = BitUtil.IsBitSet(secondOperand, 31);

                        break;
                    case 0b11: // ROR
                        if (isZeroSpecialCase)
                        {
                            secondOperand = BitUtil.RotateRight(secondOperand, 1);

                            if (CurrentStatus.Carry)
                            {
                                BitUtil.SetBit(ref secondOperand, 31);
                            }
                            else
                            {
                                BitUtil.ClearBit(ref secondOperand, 31);
                            }
                        }
                        else
                        {
                            secondOperand = BitUtil.RotateRight(secondOperand, shift);

                            CurrentStatus.Carry = BitUtil.IsBitSet(secondOperand, shift - 1);
                        }

                        break;
                }
            }

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

    }
}