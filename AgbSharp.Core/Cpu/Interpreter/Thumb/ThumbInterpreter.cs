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

    }
}