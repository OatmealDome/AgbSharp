namespace AgbSharp.Core.Cpu.Register
{
    public interface IRegisterSet
    {
        ref uint GetRegister(int reg);

    }
}