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

            return 0;
        }

    }
}