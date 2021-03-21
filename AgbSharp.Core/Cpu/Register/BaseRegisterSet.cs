namespace AgbSharp.Core.Cpu.Register
{
    class BaseRegisterSet : IRegisterSet
    {
        private uint[] Registers;

        public BaseRegisterSet()
        {
            // R0 to R12, R13 (SP), R14 (LR), R15 (PC)
            Registers = new uint[16];
        }

        public ref uint GetRegister(int reg)
        {
            return ref Registers[reg];
        }

    }
}