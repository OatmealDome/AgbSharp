using System.Collections.Generic;
using AgbSharp.Core.Cpu.Interpreter.Arm;
using AgbSharp.Core.Cpu.Register;
using AgbSharp.Core.Cpu.Status;
using AgbSharp.Core.Memory;

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

        public IRegisterSet CurrentRegisterSet; // R0 to R15
        public ProgramStatus CurrentStatus; // CPSR
        public ProgramStatus CurrentSavedStatus; // SPSR

        private ArmInterpreter ArmInterpreter;
        // private ThumbInterpreter ThumbInterpreter;

        public AgbMemoryMap MemoryMap;

        public AgbCpu(AgbMemoryMap memoryMap)
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

            ArmInterpreter = new ArmInterpreter(this);
            // ThumbInterpreter = new ThumbInterpreter(this);

            MemoryMap = memoryMap;
        }

        //
        // Helper function
        //
        private ref uint Reg(int reg)
        {
            return ref CurrentRegisterSet.GetRegister(reg);
        }

        // Step the CPU by one instruction and return the cycles it took to execute
        public int Step()
        {
            if (CurrentStatus.Thumb)
            {
                // return ThumbInterpreter.Step();
                return 0;
            }
            else
            {
                return ArmInterpreter.Step();
            }
        }

    }
}