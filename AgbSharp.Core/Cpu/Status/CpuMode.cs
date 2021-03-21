namespace AgbSharp.Core.Cpu.Status
{
    enum CpuMode : byte
    {
        // Backwards compatibility (26-bit)
        OldUser = 0x00,
        OldFastIrq = 0x01,
        OldIrq = 0x02,
        OldSupervisor = 0x03,

        // Newer Modes
        User = 0x10,
        FastIrq = 0x11,
        Irq = 0x12,
        Supervisor = 0x13,
        Abort = 0x17,
        Undefined = 0x1b,
        System = 0x1f
    }
}