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
                            // TODO
                        }
                    }
                    else
                    {
                        return FormTwelveAddPcSp(instruction);
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
                    result = PerformShift(ShiftType.LogicalLeft, dReg, (int)(sReg & 0xFF), false, true);

                    m = 1;

                    break;
                case 0b0011:
                    result = PerformShift(ShiftType.LogicalRight, dReg, (int)(sReg & 0xFF), false, true);

                    m = 1;

                    break;
                case 0b0100:
                    result = PerformShift(ShiftType.ArithmaticRight, dReg, (int)(sReg & 0xFF), false, true);

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
                    result = PerformShift(ShiftType.RotateRight, dReg, (int)(sReg & 0xFF), false, true);
                    
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
            ref uint sReg = ref Reg(sRegNum);

            int dRegNum = (BitUtil.GetBit(instruction, 7) << 3) | BitUtil.GetBitRange(instruction, 0, 2);
            ref uint dReg = ref Reg(dRegNum);

            int opcode = BitUtil.GetBitRange(instruction, 8, 9);
            switch (opcode)
            {
                case 0b00: // ADD
                    // GBATEK says that this operation does not affect CPSR
                    dReg = dReg + sReg;

                    break;
                case 0b01: // CMP
                    uint result = dReg - sReg;

                    SetCarryAndOverflowOnSubtraction(dReg, sReg, result, false);
                    
                    break;
                case 0b10:
                    dReg = sReg;
                    break;
                case 0b11:
                    uint newPc;
                    if (sRegNum == PC)
                    {
                        newPc = (uint)((Reg(PC) + 2) & ~2);
                    }
                    else
                    {
                        newPc = sReg;
                    }

                    if (!BitUtil.IsBitSet((uint)sRegNum, 0) || sRegNum == PC)
                    {
                        CurrentStatus.Thumb = false;
                    }

                    Reg(PC) = newPc;

                    break;
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

            dReg = Cpu.MemoryMap.ReadU32(pc + (uint)offset);

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
                        Cpu.MemoryMap.WriteU16(address, (ushort)(dReg & 0xFFFF));
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
                        dReg = Cpu.MemoryMap.ReadU16(address);

                        isLoad = true;

                        break;
                    case 0b11: // LDSH
                        dReg = Cpu.MemoryMap.ReadU16(address);

                        if (BitUtil.IsBitSet(dReg, 15))
                        {
                            dReg |= 0xFFFF0000;
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
                        dReg = Cpu.MemoryMap.ReadU32(address);

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
                    dReg = Cpu.MemoryMap.ReadU32(bReg + (offset * 4));

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
                dReg = Cpu.MemoryMap.ReadU16(address);

                return 1 + 1 + 1; // 1S + 1N + 1I
            }
            else
            {
                Cpu.MemoryMap.WriteU16(address, (ushort)(dReg & 0xFFFF));

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
                dReg = Cpu.MemoryMap.ReadU32(address);

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

    }
}