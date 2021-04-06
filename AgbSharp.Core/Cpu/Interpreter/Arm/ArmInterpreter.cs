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
                    else if (!BitUtil.IsBitSet(instruction, 25) // 25 to 27 is 0b000 
                            && BitUtil.IsBitSet(instruction, 7)
                            && BitUtil.IsBitSet(instruction, 4))
                    {
                        int sh = BitUtil.GetBitRange(instruction, 5, 6);
                        if (sh != 0)
                        {
                            return LoadStoreHalfSignedOperation(instruction);
                        }
                        else if (BitUtil.IsBitSet(instruction, 24))
                        {
                            return SwapOperation(instruction);
                        }
                        else
                        {
                            return MultiplyOperation(instruction);
                        }
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
                case 0b01:
                    return LoadStoreOperation(instruction);
                case 0b10: // Branch
                    if (BitUtil.IsBitSet(instruction, 25))
                    {
                        return Branch(instruction);
                    }
                    else
                    {
                        return BlockDataTransferOperation(instruction);
                    }
                case 0b11: // SWI
                    return SwiOperation(instruction);
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
                    return !CurrentStatus.Carry || CurrentStatus.Zero;
                case ArmInstructionCondition.GreaterThanOrEqual:
                    return CurrentStatus.Negative == CurrentStatus.Overflow;
                case ArmInstructionCondition.LessThan:
                    return CurrentStatus.Negative != CurrentStatus.Overflow;
                case ArmInstructionCondition.GreaterThan:
                    return !CurrentStatus.Zero && CurrentStatus.Negative == CurrentStatus.Overflow;
                case ArmInstructionCondition.LessThanOrEqual:
                    return CurrentStatus.Zero || CurrentStatus.Negative != CurrentStatus.Overflow;
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

            uint targetAddress = Reg(targetRegister);

            if (BitUtil.IsBitSet(targetAddress, 0)) // switch to Thumb
            {
                Reg(PC) = targetAddress - 1;

                CurrentStatus.Thumb = true;
            }
            else // continue in ARM
            {
                Reg(PC) = targetAddress;
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
                    operand += 8;
                }

                shift = (int)(Reg(BitUtil.GetBitRange(instruction, 8, 11)) & 0xFF);

                if (shift == 0)
                {
                    return operand;
                }
            }
            else // immediate
            {
                if (operandRegNum == PC)
                {
                    operand += 4;
                }

                shift = BitUtil.GetBitRange(instruction, 7, 11);

                if (shift == 0)
                {
                    isZeroSpecialCase = true;
                }
            }

            int shiftType = BitUtil.GetBitRange(instruction, 5, 6);
            ShiftType shiftTypeEnum;

            switch (shiftType)
            {
                case 0b00:
                    shiftTypeEnum = ShiftType.LogicalLeft;
                    break;
                case 0b01:
                    shiftTypeEnum = ShiftType.LogicalRight;
                    break;
                case 0b10:
                    shiftTypeEnum = ShiftType.ArithmaticRight;
                    break;
                case 0b11:
                    shiftTypeEnum = ShiftType.RotateRight;
                    break;
                default:
                    InterpreterAssert("Invalid shift type");

                    shiftTypeEnum = ShiftType.LogicalLeft;

                    break;
            }

            operand = PerformShift(shiftTypeEnum, operand, shift, isZeroSpecialCase, setConditionCodes);

            return operand;
        }

        private uint GetSecondOperandForAluOperation(uint instruction, bool setConditionCodes)
        {
            uint secondOperand;

            if (BitUtil.IsBitSet(instruction, 25)) // second operand is immediate
            {
                secondOperand = (uint)BitUtil.GetBitRange(instruction, 0, 7);

                // ROR
                int shift = BitUtil.GetBitRange(instruction, 8, 11) * 2;

                if (shift != 0)
                {
                    if (setConditionCodes)
                    {
                        CurrentStatus.Carry = BitUtil.IsBitSet(secondOperand, shift - 1);
                    }

                    secondOperand = BitUtil.RotateRight(secondOperand, shift);
                }
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

            int firstOperandRegNum = BitUtil.GetBitRange(instruction, 16, 19);
            uint firstOperand = Reg(firstOperandRegNum);

            if (firstOperandRegNum == PC)
            {
                if (!BitUtil.IsBitSet(instruction, 25) && BitUtil.IsBitSet(instruction, 4))
                {
                    firstOperand += 8;
                }
                else
                {
                    firstOperand += 4;
                }
            }

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

            // Save the current carry in case we need it again later
            bool lastCarry = CurrentStatus.Carry;

            uint secondOperand = GetSecondOperandForAluOperation(instruction, setConditionCodes);

            uint result;
            void SetCarryAndOverflowOnAddition(uint first, uint second)
            {
                if (!setConditionCodes)
                {
                    return;
                }

                this.SetCarryAndOverflowOnAddition(first, second, result);
            }

            void SetCarryAndOverflowOnSubtraction(uint first, uint second, bool carry)
            {
                if (!setConditionCodes)
                {
                    return;
                }

                this.SetCarryAndOverflowOnSubtraction(first, second, result, carry);
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
                    result = firstOperand + secondOperand + (uint)(lastCarry ? 1 : 0);

                    SetCarryAndOverflowOnAddition(firstOperand, secondOperand);

                    break;
                case 0b0110: // SBC
                    result = firstOperand - secondOperand + (uint)(lastCarry ? 1 : 0) - 1;
                    
                    SetCarryAndOverflowOnSubtraction(firstOperand, secondOperand, true);

                    break;
                case 0b0111: // RSC
                    result = secondOperand - firstOperand + (uint)(lastCarry ? 1 : 0) - 1;

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

            int m = 0;
            int x = 0; 
            
            int opcode = BitUtil.GetBitRange(instruction, 21, 24);
            switch (opcode)
            {
                case 0b0000: // MUL
                    dReg = mReg * sReg;

                    m = GetMBasedOnAllOnesOrZerosForMultiply(sReg);

                    break;
                case 0b0001: // MLA
                    dReg = (mReg * sReg) + nReg;

                    m = GetMBasedOnAllOnesOrZerosForMultiply(sReg);

                    x = 1;

                    break;
                case 0b0100: // UMULL
                    ulong umullResult = (ulong)mReg * sReg;

                    dReg = (uint)(umullResult >> 32);
                    nReg = (uint)(umullResult & 0xFFFFFFFF);

                    isLong = true;

                    m = GetMBasedOnAllZerosForMultiply(sReg);

                    x = 1;

                    break;
                case 0b0101: // UMLAL
                    ulong umlalAccumulate = ((ulong)dReg << 32) | nReg;

                    ulong umlalResult = ((ulong)mReg * sReg) + umlalAccumulate;

                    dReg = (uint)(umlalResult >> 32);
                    nReg = (uint)(umlalResult & 0xFFFFFFFF);

                    isLong = true;

                    m = GetMBasedOnAllZerosForMultiply(sReg);

                    x = 1;

                    break;
                case 0b0110: // SMULL
                    int mRegInt = (int)mReg;
                    int sRegInt = (int)sReg;
                    long smullResult = (long)mRegInt * (long)sRegInt;

                    dReg = (uint)(smullResult >> 32);
                    nReg = (uint)(smullResult & 0xFFFFFFFF);

                    isLong = true;

                    m = GetMBasedOnAllOnesOrZerosForMultiply(sReg);

                    x = 2;

                    break;
                case 0b0111: // SMLAL
                    long smlalAccumulate = (long)(((ulong)dReg << 32) | nReg);

                    int mRegIntSmlal = (int)mReg;
                    int sRegIntSmlal = (int)sReg;
                    long smlalResult = ((long)mRegIntSmlal * sRegIntSmlal) + smlalAccumulate;

                    dReg = (uint)(smlalResult >> 32);
                    nReg = (uint)(smlalResult & 0xFFFFFFFF);

                    isLong = true;

                    m = GetMBasedOnAllOnesOrZerosForMultiply(sReg);

                    x = 2;

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

            return 1 + (m + x); // 1S + (m + x)I
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

                uint psr = programStatus.RegisterValue;

                if (BitUtil.IsBitSet(instruction, 19))
                {
                    psr = (psr & 0x00FFFFFF) | secondOperand & 0xFF000000;
                }
                
                if (BitUtil.IsBitSet(instruction, 18))
                {
                    psr = (psr & 0xFF00FFFF) | secondOperand & 0x00FF0000;
                }

                if (BitUtil.IsBitSet(instruction, 17))
                {
                    psr = (psr & 0xFFFF00FF) | secondOperand & 0x0000FF00;
                }

                if (BitUtil.IsBitSet(instruction, 16))
                {
                    psr = (psr & 0xFFFFFF00) | secondOperand & 0x000000FF;
                }

                programStatus.RegisterValue = psr;
            }
            else // MRS
            {
                ref uint destinationReg = ref Reg(BitUtil.GetBitRange(instruction, 12, 15));

                destinationReg = programStatus.RegisterValue;
            }

            return 1; // 1S
        }

        private int LoadStoreOperation(uint instruction)
        {
            int nRegNum = BitUtil.GetBitRange(instruction, 16, 19);
            uint nReg = Reg(nRegNum);

            if (nRegNum == PC)
            {
                nReg += 4;
            }

            int dRegNum = BitUtil.GetBitRange(instruction, 12, 15);

            bool isImmediateOffset = !BitUtil.IsBitSet(instruction, 25);
            bool isPreIndex = BitUtil.IsBitSet(instruction, 24);
            bool isUp = BitUtil.IsBitSet(instruction, 23);
            bool isByte = BitUtil.IsBitSet(instruction, 22);
            bool isWriteBack = BitUtil.IsBitSet(instruction, 21);
            bool isLoad = BitUtil.IsBitSet(instruction, 20);

            // For information about pre-/post-load and write back:
            // https://www.cs.uregina.ca/Links/class-info/301/ARM-addressing/lecture.html

            uint offset;
            if (isImmediateOffset)
            {
                offset = (uint)BitUtil.GetBitRange(instruction, 0, 11);
            }
            else
            {
                offset = GetOperandByShiftingRegister(instruction, false);
            }

            uint address;
            if (isUp)
            {
                address = nReg + offset;
            }
            else
            {
                address = nReg - offset;
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

            if (isWriteBack || !isPreIndex)
            {
                Reg(nRegNum) = address;
            }

            if (isLoad)
            {
                ref uint dReg = ref Reg(dRegNum);

                if (isByte)
                {
                    dReg = Cpu.MemoryMap.Read(effectiveAddress);
                }
                else
                {
                    // Check if aligned on a half-word boundary
                    if (effectiveAddress % 4 != 0 && effectiveAddress % 2 == 0)
                    {
                        // Read the half-word at this address, but don't touch the higher bits
                        // (on a real ARM CPU, they will be garbage).
                        dReg = Cpu.MemoryMap.ReadU16(effectiveAddress);
                    }
                    else
                    {
                        dReg = Cpu.MemoryMap.ReadU32(effectiveAddress);
                    }
                }

                if (dRegNum == PC)
                {
                    return 2 + 2 + 1; // 2S + 2N + 1I
                }
                else
                {
                    return 1 + 1 + 1; // 1S + 1N + 1I
                }
            }
            else
            {
                uint dReg;
                if (dRegNum == PC)
                {
                    dReg = Reg(PC) + 12;
                }
                else
                {
                    dReg = Reg(dRegNum);
                }

                if (isByte)
                {
                    Cpu.MemoryMap.Write(effectiveAddress, (byte)dReg);
                }
                else
                {
                    // Mask off the lower bits to force a word-aligned address
                    effectiveAddress &= 0xFFFFFFC;
                
                    Cpu.MemoryMap.WriteU32(effectiveAddress, dReg);
                }

                return 2; // 2N
            }
        }

        private int LoadStoreHalfSignedOperation(uint instruction)
        {
            ref uint nReg = ref Reg(BitUtil.GetBitRange(instruction, 16, 19));

            int dRegNum = BitUtil.GetBitRange(instruction, 12, 15);
            ref uint dReg = ref Reg(dRegNum);


            bool isImmediateOffset = !BitUtil.IsBitSet(instruction, 25);
            bool isPreIndex = BitUtil.IsBitSet(instruction, 24);
            bool isUp = BitUtil.IsBitSet(instruction, 23);
            bool isWriteBack = BitUtil.IsBitSet(instruction, 21);
            bool isLoad = BitUtil.IsBitSet(instruction, 20);

            uint offset;
            if (BitUtil.IsBitSet(instruction, 22))
            {
                uint loNybble = (uint)BitUtil.GetBitRange(instruction, 0, 3);
                uint hiNybble = (uint)BitUtil.GetBitRange(instruction, 8, 11);

                offset = hiNybble << 4 | loNybble;
            }
            else
            {
                offset = Reg(BitUtil.GetBitRange(instruction, 0, 3));
            }

            uint address;
            if (isUp)
            {
                address = nReg + offset;
            }
            else
            {
                address = nReg - offset;
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

            if (isWriteBack || !isPreIndex)
            {
                nReg = address;
            }

            int opType = BitUtil.GetBitRange(instruction, 5, 6);
            InterpreterAssert(opType != 0b00, "SWP not handled here");

            if (isLoad)
            {
                switch (opType)
                {
                    case 0b01:
                        dReg = Cpu.MemoryMap.ReadU16(effectiveAddress);
                        break;
                    case 0b10:
                        dReg = Cpu.MemoryMap.Read(effectiveAddress);

                        if (BitUtil.IsBitSet(dReg, 7))
                        {
                            dReg |= 0xFFFFFF00;
                        }

                        break;
                    case 0b11:
                        dReg = Cpu.MemoryMap.ReadU16(effectiveAddress);

                        if (BitUtil.IsBitSet(dReg, 15))
                        {
                            dReg |= 0xFFFF0000;
                        }

                        break;
                }

                if (dRegNum == PC)
                {
                    return 2 + 2 + 1; // 2S + 2N + 1I
                }
                else
                {
                    return 1 + 1 + 1; // 1S + 1N + 1I
                }
            }
            else
            {
                InterpreterAssert(opType == 0b01, "LDRD/STRD not implemented");

                Cpu.MemoryMap.WriteU16(effectiveAddress, (ushort)dReg);

                return 2; // 2S
            }
        }

        private int BlockDataTransferOperation(uint instruction)
        {
            int nRegNum = BitUtil.GetBitRange(instruction, 16, 19);
            ref uint nReg = ref Reg(nRegNum);

            bool isPreIndex = BitUtil.IsBitSet(instruction, 24);
            bool isUp = BitUtil.IsBitSet(instruction, 23);
            bool sFlag = BitUtil.IsBitSet(instruction, 22);
            bool isWriteBack = BitUtil.IsBitSet(instruction, 21);
            bool isLoad = BitUtil.IsBitSet(instruction, 20);

            bool useUserBank = false;

            if (sFlag && isLoad && BitUtil.IsBitSet(instruction, PC))
            {
                CurrentStatus.RegisterValue = SavedStatus.RegisterValue;
            }
            else
            {
                useUserBank = true;
            }

            uint bitfield = instruction & 0xFFFF;

            int transferredWords = PerformDataBlockTransfer(ref nReg, isPreIndex, isUp, isWriteBack, isLoad, useUserBank, bitfield);

            if (isLoad)
            {
                if (nRegNum == PC)
                {
                    return (transferredWords + 1) + 2 + 1; // (n+1)S + 2N + 1I
                }
                else
                {
                    return (transferredWords) + 1 + 1; // nS + 1S + 1I
                }
            }
            else
            {
                return (transferredWords - 1) + 2; // (n-1)S + 2N
            }
        }

        private int SwapOperation(uint instruction)
        {
            bool isByte = BitUtil.IsBitSet(instruction, 22);

            ref uint nReg = ref Reg(BitUtil.GetBitRange(instruction, 16, 19));
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 12, 15));
            ref uint mReg = ref Reg(BitUtil.GetBitRange(instruction, 0, 3));

            uint value;
            if (isByte)
            {
                value = Cpu.MemoryMap.Read(nReg);

                Cpu.MemoryMap.Write(nReg, (byte)(mReg & 0xFF));
            }
            else
            {
                value = Cpu.MemoryMap.ReadU32(nReg);

                Cpu.MemoryMap.WriteU32(nReg, mReg);
            }

            dReg = value;

            return 1 + 2 + 1; // 1S + 2N + 1I            
        }

        private int SwiOperation(uint instruction)
        {
            PerformSwi();

            return 2 + 1; // 2S + 1N
        }

    }
}