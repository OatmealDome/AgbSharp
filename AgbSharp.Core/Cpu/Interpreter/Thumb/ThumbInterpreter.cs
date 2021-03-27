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
                    return MoveShiftedRegister(instruction);
            }

            InterpreterAssert($"Invalid instruction ({instruction:x4})");

            return 0;
        }

        private int MoveShiftedRegister(uint instruction)
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

    }
}