using AgbSharp.Core.Cpu.Status;
using AgbSharp.Core.Util;

namespace AgbSharp.Core.Cpu.Interpreter.Thumb
{
    class ThumbInterpreter : InstructionSetInterpreter
    {
        public ThumbInterpreter(AgbCpu cpu) : base(cpu)
        {

        }

        public override int Step()
        {
            uint instruction = Cpu.MemoryMap.ReadU16(Reg(PC));
            
            Reg(PC) += 2;

            int type = BitUtil.GetBitRange(instruction, 13, 15);
            switch (type)
            {
                case 0b000:
                    if (BitUtil.GetBitRange(instruction, 11, 12) == 0b11)
                    {
                        return FormTwoAddSubtractOperation(instruction);
                    }
                    else
                    {
                        return FormOneMoveShiftedRegister(instruction);
                    }
                case 0b001:
                    return FormThreeAluOperation(instruction);
                case 0b010:
                    if (BitUtil.IsBitSet(instruction, 12))
                    {
                        return FormSevenEightLoadStore(instruction);
                    }
                    else if (BitUtil.IsBitSet(instruction, 11))
                    {
                        return FormSixPcRelativeLoad(instruction);
                    }
                    else if (BitUtil.IsBitSet(instruction, 10))
                    {
                        return FormFiveRegisterOperations(instruction);
                    }
                    else
                    {
                        return FormFourAluOperation(instruction);
                    }
                case 0b011:
                    return FormNineLoadStore(instruction);
                case 0b100:
                    if (BitUtil.IsBitSet(instruction, 12))
                    {
                        return FormElevenLoadStore(instruction);
                    }
                    else
                    {
                        return FormTenLoadStore(instruction);
                    }
                case 0b101:
                    if (BitUtil.IsBitSet(instruction, 12))
                    {
                        int subtype = BitUtil.GetBitRange(instruction, 9, 10);

                        switch (subtype)
                        {
                            case 0b00:
                                return FormThirteenAddSpOffset(instruction);
                            case 0b10:
                                if (BitUtil.IsBitSet(instruction, 11))
                                {
                                    return FormFourteenPopOperation(instruction);
                                }
                                else
                                {
                                    return FormFourteenPushOperation(instruction);
                                }
                        }
                    }
                    else
                    {
                        return FormTwelveAddPcSp(instruction);
                    }

                    break;
                case 0b110:
                    if (BitUtil.IsBitSet(instruction, 12))
                    {
                        if (BitUtil.GetBitRange(instruction, 8, 11) == 0b1111)
                        {
                            return FormSeventeenSwiOperation(instruction);
                        }
                        else 
                        {
                            return FormSixteenConditionalBranch(instruction);
                        }
                    }
                    else
                    {
                        return FormFifteenBlockTransfer(instruction);
                    }
                case 0b111:
                    int opcode = BitUtil.GetBitRange(instruction, 11, 12);

                    switch (opcode)
                    {
                        case 0b00:
                            return FormEighteenUnconditionalBranch(instruction);
                        case 0b10:
                            return FormNineteenLoadHiBits(instruction);
                        case 0b11:
                            return FormNineteenExecuteBranch(instruction);
                    }

                    break;
            }

            InterpreterAssert($"Invalid instruction ({instruction:x4})");

            return 0;
        }

        private int FormOneMoveShiftedRegister(uint instruction)
        {
            ref uint sReg = ref Reg(BitUtil.GetBitRange(instruction, 3, 5));
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 0, 2));

            int shift = BitUtil.GetBitRange(instruction, 6, 10);

            int shiftType = BitUtil.GetBitRange(instruction, 11, 12);
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
                default:
                    InterpreterAssert("Invalid shift type");

                    shiftTypeEnum = ShiftType.LogicalLeft;

                    break;
            }

            dReg = PerformShift(shiftTypeEnum, sReg, shift, shift == 0, true);

            CurrentStatus.Zero = dReg == 0;
            CurrentStatus.Negative = BitUtil.IsBitSet(dReg, 31);

            return 1; // 1S
        }

        private int FormTwoAddSubtractOperation(uint instruction)
        {
            ref uint sReg = ref Reg(BitUtil.GetBitRange(instruction, 3, 5));
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 0, 2));

            uint operand;
            if (BitUtil.IsBitSet(instruction, 10)) // immediate
            {
                operand = (uint)BitUtil.GetBitRange(instruction, 6, 8);
            }
            else
            {
                operand = Reg(BitUtil.GetBitRange(instruction, 6, 8));
            }

            if (BitUtil.IsBitSet(instruction, 9)) // subtraction
            {
                dReg = sReg - operand;

                SetCarryAndOverflowOnSubtraction(sReg, operand, dReg, false);
            }
            else
            {
                dReg = sReg + operand;

                SetCarryAndOverflowOnAddition(sReg, operand, dReg);
            }

            CurrentStatus.Zero = dReg == 0;
            CurrentStatus.Negative = BitUtil.IsBitSet(dReg, 31);

            return 1; // 1S
        }

        private int FormThreeAluOperation(uint instruction)
        {
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 8, 10));

            uint immediate = (uint)BitUtil.GetBitRange(instruction, 0, 7);

            uint result;

            int opcode = BitUtil.GetBitRange(instruction, 11, 12);
            switch (opcode)
            {
                case 0b00:
                    result = immediate;
                    break;
                case 0b01:
                case 0b11:
                    result = dReg - immediate;

                    SetCarryAndOverflowOnSubtraction(dReg, immediate, result, false);

                    break;
                case 0b10:
                    result = dReg + immediate;

                    SetCarryAndOverflowOnAddition(dReg, immediate, result);

                    break;
                default:
                    InterpreterAssert($"Invalid opcode for Form Three (${opcode})");

                    result = 0;

                    break;
            }

            CurrentStatus.Zero = result == 0;
            CurrentStatus.Negative = BitUtil.IsBitSet(result, 31);

            if (opcode != 0b01) // CMP
            {
                dReg = result;
            }

            return 1; // 1S
        }

        private int FormFourAluOperation(uint instruction)
        {
            ref uint sReg = ref Reg(BitUtil.GetBitRange(instruction, 3, 5));
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 0, 2));

            // for instruction timing, only some instructions set this
            int m = 0;

            uint result;

            int opcode = BitUtil.GetBitRange(instruction, 6, 9);
            switch (opcode)
            {
                case 0b0000:
                case 0b1000:
                    result = dReg & sReg;
                    break;
                case 0b0001:
                    result = dReg ^ sReg;
                    break;
                case 0b0010:
                    int lslShift = (int)(sReg & 0xFF);

                    result = PerformShift(ShiftType.LogicalLeft, dReg, lslShift, false, lslShift != 0);

                    m = 1;

                    break;
                case 0b0011:
                    int lsrShift = (int)(sReg & 0xFF);

                    result = PerformShift(ShiftType.LogicalRight, dReg, lsrShift, false, lsrShift != 0);

                    m = 1;

                    break;
                case 0b0100:
                    int asrShift = (int)(sReg & 0xFF);

                    result = PerformShift(ShiftType.ArithmaticRight, dReg, asrShift, false, asrShift != 0);

                    m = 1;

                    break;
                case 0b0101:
                    result = dReg + sReg + (uint)(CurrentStatus.Carry ? 1 : 0);

                    SetCarryAndOverflowOnAddition(dReg, sReg, result);

                    break;
                case 0b0110:
                    result = dReg - sReg + (uint)(CurrentStatus.Carry ? 1 : 0) - 1;
                    
                    SetCarryAndOverflowOnSubtraction(dReg, sReg, result, true);

                    break;
                case 0b0111:
                    int rorShift = (int)(sReg & 0xFF);

                    result = PerformShift(ShiftType.RotateRight, dReg, rorShift, false, rorShift != 0);
                    
                    m = 1;

                    break;
                case 0b1001:
                    result = 0 - sReg;

                    SetCarryAndOverflowOnSubtraction(0, sReg, result, false);

                    break;
                case 0b1010:
                    result = dReg - sReg;

                    SetCarryAndOverflowOnSubtraction(dReg, sReg, result, false);

                    break;
                case 0b1011:
                    result = dReg + sReg;

                    SetCarryAndOverflowOnAddition(dReg, sReg, result);

                    break;
                case 0b1100:
                    result = dReg | sReg;
                    break;
                case 0b1101:
                    result = dReg * sReg;

                    m = GetMBasedOnAllOnesOrZerosForMultiply(dReg);

                    break;
                case 0b1110:
                    result = dReg & ~sReg;
                    break;
                case 0b1111:
                    result = ~sReg;
                    break;
                default:
                    InterpreterAssert($"Invalid opcode in Form Four ({opcode:x})");

                    result = 0;

                    break;
            }

            if (opcode != 0b1000 && opcode != 0b1010 && opcode != 0b1011)
            {
                dReg = result;
            }

            CurrentStatus.Zero = result == 0;
            CurrentStatus.Negative = BitUtil.IsBitSet(result, 31);

            return 1 + m; // 1S + mI
        }

        private int FormFiveRegisterOperations(uint instruction)
        {
            int sRegNum = (BitUtil.GetBit(instruction, 6) << 3) | BitUtil.GetBitRange(instruction, 3, 5);
            uint sReg = Reg(sRegNum);

            if (sRegNum == PC)
            {
                sReg += 2;
            }

            int dRegNum = (BitUtil.GetBit(instruction, 7) << 3) | BitUtil.GetBitRange(instruction, 0, 2);
            ref uint dReg = ref Reg(dRegNum);

            int opcode = BitUtil.GetBitRange(instruction, 8, 9);
            switch (opcode)
            {
                case 0b00: // ADD
                    // GBATEK says that this operation does not affect CPSR
                    dReg = dReg + sReg;

                    if (dRegNum == PC)
                    {
                        dReg += 2;
                    }

                    break;
                case 0b01: // CMP
                    uint firstOperand = dReg;

                    if (dRegNum == PC)
                    {
                        firstOperand += 2;
                    }

                    uint result = firstOperand - sReg;

                    SetCarryAndOverflowOnSubtraction(firstOperand, sReg, result, false);

                    CurrentStatus.Zero = result == 0;
                    CurrentStatus.Negative = BitUtil.IsBitSet(result, 31);
                    
                    break;
                case 0b10:
                    dReg = sReg;
                    break;
                case 0b11:
                    uint newPc;
                    if (sRegNum == PC)
                    {
                        newPc = (uint)(sReg & ~2);

                        CurrentStatus.Thumb = false;
                    }
                    else
                    {
                        newPc = sReg;

                        if (!BitUtil.IsBitSet(newPc, 0))
                        {
                            CurrentStatus.Thumb = false;
                        }
                        else
                        {
                            newPc--;
                        }
                    }

                    Reg(PC) = newPc;

                    break;
            }

            if (dRegNum == PC)
            {
                dReg = dReg & 0xFFFFFFFE;
            }

            if (opcode == 0b01)
            {
                return 1; // 1S
            }
            else
            {
                if (opcode == 0b11 || dReg == PC)
                {
                    return 2 + 1; // 2S + 1N
                }
                else
                {
                    return 1; // 1S
                }
            }
        }

        private int FormSixPcRelativeLoad(uint instruction)
        {
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 8, 10));

            uint pc = (uint)((Reg(PC) + 2) & ~2);

            int offset = BitUtil.GetBitRange(instruction, 0, 7) * 4;

            dReg = LoadWordFromAddress(pc + (uint)offset);

            return 1 + 1 + 1; // 1S + 1N + 1I
        }

        private int FormSevenEightLoadStore(uint instruction)
        {
            ref uint oReg = ref Reg(BitUtil.GetBitRange(instruction, 6, 8));
            ref uint bReg = ref Reg(BitUtil.GetBitRange(instruction, 3, 5));
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 0, 2));

            bool isLoad = false;

            uint address = bReg + oReg;

            int opcode = BitUtil.GetBitRange(instruction, 10, 11);

            if (BitUtil.IsBitSet(instruction, 9)) // Form Eight (half-words and sign extension)
            {
                switch (opcode)
                {
                    case 0b00: // STRH
                        // Mask bit 0 to force an aligned write
                        Cpu.MemoryMap.WriteU16(address & 0xFFFFFFFE, (ushort)(dReg & 0xFFFF));
                        break;
                    case 0b01: // LDSB
                        dReg = Cpu.MemoryMap.Read(address);

                        if (BitUtil.IsBitSet(dReg, 7))
                        {
                            dReg |= 0xFFFFFF00;
                        }

                        isLoad = true;

                        break;
                    case 0b10: // LDRH
                        dReg = LoadHalfWordFromAddress(address);

                        isLoad = true;

                        break;
                    case 0b11: // LDSH
                        dReg = LoadHalfWordFromAddress(address);

                        if (address % 2 != 0)
                        {
                            // Sign extend based on bit 7 if unaligned
                            // https://github.com/mgba-emu/mgba/commit/4bd7a65432c97b8909833f72b6428cfacea65b41
                            if (BitUtil.IsBitSet(dReg, 7))
                            {
                                dReg |= 0xFFFFFF00;
                            }
                        }
                        else
                        {
                            if (BitUtil.IsBitSet(dReg, 15))
                            {
                                dReg |= 0xFFFF0000;
                            }
                        }

                        isLoad = true;

                        break;
                }
            }
            else
            {
                switch (opcode)
                {
                    case 0b00: // STR
                        Cpu.MemoryMap.WriteU32(address, dReg);
                        break;
                    case 0b01: // STRB
                        Cpu.MemoryMap.Write(address, (byte)(dReg & 0xFF));
                        break;
                    case 0b10: // LDR
                        dReg = LoadWordFromAddress(address);

                        isLoad = true;

                        break;
                    case 0b11: // LDRB
                        dReg = Cpu.MemoryMap.Read(address);

                        isLoad = true;

                        break;
                }
            }

            if (isLoad)
            {
                return 1 + 1 + 1; // 1S + 1N + 1I
            }
            else
            {
                return 2; // 2S;
            }
        }

        private int FormNineLoadStore(uint instruction)
        {
            ref uint bReg = ref Reg(BitUtil.GetBitRange(instruction, 3, 5));
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 0, 2));

            bool isLoad = false;

            uint offset = (uint)BitUtil.GetBitRange(instruction, 6, 10);

            int opcode = BitUtil.GetBitRange(instruction, 11, 12);
            switch (opcode)
            {
                case 0b00: // STR
                    Cpu.MemoryMap.WriteU32(bReg + (offset * 4), dReg);
                    break;
                case 0b01: // LDR
                    dReg = LoadWordFromAddress(bReg + (offset * 4));

                    isLoad = true;

                    break;
                case 0b10: // STRB
                    Cpu.MemoryMap.Write(bReg + offset, (byte)(dReg & 0xFF));
                    break;
                case 0b11: // LDRB
                    dReg = Cpu.MemoryMap.Read(bReg + offset);

                    isLoad = true;

                    break;
            }

            if (isLoad)
            {
                return 1 + 1 + 1; // 1S + 1N + 1I
            }
            else
            {
                return 2; // 2S;
            }
        }

        private int FormTenLoadStore(uint instruction)
        {
            ref uint bReg = ref Reg(BitUtil.GetBitRange(instruction, 3, 5));
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 0, 2));

            uint offset = (uint)BitUtil.GetBitRange(instruction, 6, 10);
            uint address = bReg + (offset * 2);

            if (BitUtil.IsBitSet(instruction, 11))
            {
                dReg = LoadHalfWordFromAddress(address);

                return 1 + 1 + 1; // 1S + 1N + 1I
            }
            else
            {
                // Mask bit 0 to force an aligned write
                Cpu.MemoryMap.WriteU16(address & 0xFFFFFFFE, (ushort)(dReg & 0xFFFF));

                return 2; // 2S
            }
        }

        private int FormElevenLoadStore(uint instruction)
        {
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 8, 10));

            uint offset = (uint)BitUtil.GetBitRange(instruction, 0, 7);
            uint address = Reg(SP) + (offset * 4);

            if (BitUtil.IsBitSet(instruction, 11))
            {
                dReg = LoadWordFromAddress(address);

                return 1 + 1 + 1; // 1S + 1N + 1I
            }
            else
            {
                Cpu.MemoryMap.WriteU32(address, dReg);

                return 2; // 2S
            }
        }
        
        private int FormTwelveAddPcSp(uint instruction)
        {
            ref uint dReg = ref Reg(BitUtil.GetBitRange(instruction, 8, 10));

            uint offset = (uint)BitUtil.GetBitRange(instruction, 0, 7) * 4;

            if (BitUtil.IsBitSet(instruction, 11)) // ADD Rd, SP, #imm
            {
                dReg = Reg(SP) + offset;
            }
            else // ADD Rd, PC, #imm
            {
                uint pc = (uint)((Reg(PC) + 2) & ~2);

                dReg = pc + offset;
            }

            return 1; // 1S
        }

        private int FormThirteenAddSpOffset(uint instruction)
        {
            uint offset = (uint)BitUtil.GetBitRange(instruction, 0, 6) * 4;

            uint sp = Reg(SP);
            if (BitUtil.IsBitSet(instruction, 7))
            {
                sp -= offset;
            }
            else
            {
                sp += offset;
            }

            Reg(SP) = sp;

            return 1; // 1S
        }

        private int FormFourteenPushOperation(uint instruction)
        {
            uint bitfield = instruction & 0xFF;

            if (BitUtil.IsBitSet(instruction, 8)) // PUSH LR
            {
                BitUtil.SetBit(ref bitfield, LR);
            }

            int transferredWords = PerformDataBlockTransfer(SP, true, false, true, false, false, bitfield);

            return (transferredWords - 1) + 2; // (n - 1)S + 2N
        }

        private int FormFourteenPopOperation(uint instruction)
        {
            uint bitfield = instruction & 0xFF;

            bool popPc = BitUtil.IsBitSet(instruction, 8); // POP PC
            if (popPc)
            {
                BitUtil.SetBit(ref bitfield, PC);
            }

            int transferredWords = PerformDataBlockTransfer(SP, false, true, true, true, false, bitfield);

            if (popPc)
            {
                return (transferredWords + 1) + 2 + 1; // (n + 1)S + 2N + 1I
            }
            else
            {
                return transferredWords + 1 + 1; // nS + 1N + 1I
            }
        }

        private int FormFifteenBlockTransfer(uint instruction)
        {
            int bRegNum = BitUtil.GetBitRange(instruction, 8, 10);

            uint bitfield = instruction & 0xFF;

            bool isLoad = BitUtil.IsBitSet(instruction, 11);

            int transferredWords = PerformDataBlockTransfer(bRegNum, false, true, true, isLoad, false, bitfield);

            if (isLoad)
            {
                return transferredWords + 1 + 1; // nS + 1N + 1I
            }
            else
            {
                return (transferredWords - 1) + 2; // (n - 1)S + 2N
            }
        }

        private int FormSixteenConditionalBranch(uint instruction)
        {
            uint offsetRaw = (uint)BitUtil.GetBitRange(instruction, 0, 7);

            // Sign-extend
            if (BitUtil.IsBitSet(offsetRaw, 7))
            {
                offsetRaw |= 0xFFFFFF00;
            }

            int offset = (int)offsetRaw;

            bool condition = false;

            int opcode = BitUtil.GetBitRange(instruction, 8, 11);
            switch (opcode)
            {
                case 0:
                    condition = CurrentStatus.Zero;
                    break;
                case 1:
                    condition = !CurrentStatus.Zero;
                    break;
                case 2:
                    condition = CurrentStatus.Carry;
                    break;
                case 3:
                    condition = !CurrentStatus.Carry;
                    break;
                case 4:
                    condition = CurrentStatus.Negative;
                    break;
                case 5:
                    condition = !CurrentStatus.Negative;
                    break;
                case 6:
                    condition = CurrentStatus.Overflow;
                    break;
                case 7:
                    condition = !CurrentStatus.Overflow;
                    break;
                case 8:
                    condition = CurrentStatus.Carry && !CurrentStatus.Zero;
                    break;
                case 9:
                    condition = !CurrentStatus.Carry || CurrentStatus.Zero;
                    break;
                case 0xA:
                    condition = CurrentStatus.Negative == CurrentStatus.Overflow;
                    break;
                case 0xB:
                    condition = CurrentStatus.Negative != CurrentStatus.Overflow;
                    break;
                case 0xC:
                    condition = !CurrentStatus.Zero && CurrentStatus.Negative == CurrentStatus.Overflow;
                    break;
                case 0xD:
                    condition = CurrentStatus.Zero || CurrentStatus.Negative != CurrentStatus.Overflow;
                    break;
                default:
                    InterpreterAssert($"Invalid condition for Branch ({opcode:x})");
                    break;
            }

            if (condition)
            {
                Reg(PC) = (uint)(Reg(PC) + 2 + (offset * 2));

                return 2 + 1; // 2S + 1N
            }
            else
            {
                return 1; // S
            }
        }

        private int FormSeventeenSwiOperation(uint instruction)
        {
            PerformSwi();

            return 2 + 1; // 2S + 1N
        }

        private int FormEighteenUnconditionalBranch(uint instruction)
        {
            uint offsetRaw = (uint)BitUtil.GetBitRange(instruction, 0, 10);

            // Sign-extend
            if (BitUtil.IsBitSet(offsetRaw, 10))
            {
                offsetRaw |= 0xFFFFF800;
            }

            int signedOffset = (int)offsetRaw;

            Reg(PC) = (uint)(Reg(PC) + 2 + (signedOffset * 2));

            return 2 + 1; // 2S + 1N
        }

        private int FormNineteenLoadHiBits(uint instruction)
        {
            uint hiBits = (uint)BitUtil.GetBitRange(instruction, 0, 10) << 12;

            // Sign-extend if negative
            if (BitUtil.IsBitSet(hiBits, 22))
            {
                hiBits |= 0xFF800000;
            }
            
            Reg(LR) = Reg(PC) + 2 + hiBits;

            return 1; // 1S
        }

        private int FormNineteenExecuteBranch(uint instruction)
        {
            uint loBits = (uint)BitUtil.GetBitRange(instruction, 0, 10) << 1;

            ref uint pc = ref Reg(PC);
            ref uint lr = ref Reg(LR);

            uint newLr = pc | 1;

            pc = lr + loBits;

            lr = newLr;

            return 2 + 1; // 2S + 1N
        }

    }
}