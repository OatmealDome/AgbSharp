namespace AgbSharp.Core.Cpu.Register
{
    class FastIrqRegisterSet : IRegisterSet
    {
        private readonly IRegisterSet BaseSet;

        private uint[] OverrideRegisters;

        public FastIrqRegisterSet(IRegisterSet baseSet)
        {
            BaseSet = baseSet;

            OverrideRegisters = new uint[7];
        }

        public ref uint GetRegister(int reg)
        {
            if (reg < 8 || reg == 15) // PC
            {
                return ref BaseSet.GetRegister(reg);
            }
            else
            {
                return ref OverrideRegisters[reg - 8];
            }
        }
        
    }
}