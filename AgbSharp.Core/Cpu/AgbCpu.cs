using System.Collections.Generic;
using AgbSharp.Core.Cpu.Register;
using AgbSharp.Core.Cpu.Status;

namespace AgbSharp.Core.Cpu
{
    //
    // The AGB uses an ARM7TDMI chip which implements the ARMv4 and Thumb (original)
    // instruction set. There are no co-processors.
    //
    // Some ARMv5+ instructions are implemented here as GBATEK documentation contains
    // information on both in one area.
    //
    class AgbCpu
    {
        // Constants to use with the Reg() helper
        private const int SP = 13;
        private const int LR = 14;
        private const int PC = 15;

        private Dictionary<CpuMode, IRegisterSet> RegisterSets;
        private Dictionary<CpuMode, ProgramStatus> SavedStatuses;

        private IRegisterSet CurrentRegisterSet; // R0 to R15
        private ProgramStatus CurrentStatus; // CPSR
        private ProgramStatus CurrentSavedStatus; // SPSR

        public AgbCpu()
        {
            RegisterSets = new Dictionary<CpuMode, IRegisterSet>();

            IRegisterSet baseSet = new BaseRegisterSet();
            RegisterSets[CpuMode.User] = baseSet;
            RegisterSets[CpuMode.System] = baseSet;
            RegisterSets[CpuMode.FastIrq] = new FastIrqRegisterSet(baseSet);
            RegisterSets[CpuMode.Irq] = new OverrideRegisterSet(baseSet);
            RegisterSets[CpuMode.Supervisor] = new OverrideRegisterSet(baseSet);
            RegisterSets[CpuMode.Abort] = new OverrideRegisterSet(baseSet);
            RegisterSets[CpuMode.Undefined] = new OverrideRegisterSet(baseSet);

            SavedStatuses = new Dictionary<CpuMode, ProgramStatus>();

            SavedStatuses[CpuMode.System] = new ProgramStatus();
            SavedStatuses[CpuMode.FastIrq] = new ProgramStatus();
            SavedStatuses[CpuMode.Irq] = new ProgramStatus();
            SavedStatuses[CpuMode.Supervisor] = new ProgramStatus();
            SavedStatuses[CpuMode.Abort] = new ProgramStatus();
            SavedStatuses[CpuMode.Undefined] = new ProgramStatus();

            CurrentRegisterSet = RegisterSets[CpuMode.User];
            CurrentStatus = new ProgramStatus();
            CurrentSavedStatus = null;
        }

        // Helper function
        private ref uint Reg(int reg)
        {
            return ref CurrentRegisterSet.GetRegister(reg);
        }

    }
}