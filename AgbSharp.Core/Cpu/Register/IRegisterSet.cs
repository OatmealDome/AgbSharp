namespace AgbSharp.Core.Cpu.Register
{
    interface IRegisterSet
    {
        ref uint GetRegister(int reg);

    }
}