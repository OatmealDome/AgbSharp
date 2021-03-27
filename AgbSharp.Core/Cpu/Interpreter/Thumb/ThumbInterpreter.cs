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

            InterpreterAssert($"Invalid instruction ({instruction:x4})");

            return 0;
        }

    }
}