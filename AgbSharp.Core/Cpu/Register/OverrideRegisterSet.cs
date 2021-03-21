namespace AgbSharp.Core.Cpu.Register
{
    class OverrideRegisterSet : IRegisterSet
    {
        private readonly IRegisterSet BaseSet;

        private uint OverrideSp;
        private uint OverrideLr;

        public OverrideRegisterSet(IRegisterSet baseSet)
        {
            BaseSet = baseSet;
        }

        public ref uint GetRegister(int reg)
        {
            if (reg == 13) // SP
            {
                return ref OverrideSp;
            }
            else if (reg == 14) // LR
            {
                return ref OverrideLr;
            }
            else
            {
                return ref BaseSet.GetRegister(reg);
            }
        }

    }
}