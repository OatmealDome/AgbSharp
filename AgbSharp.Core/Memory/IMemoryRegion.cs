namespace AgbSharp.Core.Memory
{
    public interface IMemoryRegion
    {
        byte Read(uint address);

        void Write(uint address, byte val);

    }
}