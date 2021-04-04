using System.Collections.Generic;
using AgbSharp.Core.Cpu.Interpreter.Arm;
using AgbSharp.Core.Cpu.Interpreter.Thumb;
using AgbSharp.Core.Cpu.Interrupt;
using AgbSharp.Core.Cpu.Register;
using AgbSharp.Core.Cpu.Status;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Util;

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

        public IRegisterSet CurrentRegisterSet
        {
            get
            {
                return RegisterSets[CurrentStatus.Mode];
            }
        }

        // CPSR
        public ProgramStatus CurrentStatus;

        // SPSR
        public ProgramStatus CurrentSavedStatus
        {
            get
            {
                return SavedStatuses[CurrentStatus.Mode];
            }
        }

        // Interrupts
        private bool InterruptMasterEnable;
        private uint EnabledInterrupts;
        private uint AcknowledgedInterrupts;

        private ArmInterpreter ArmInterpreter;
        private ThumbInterpreter ThumbInterpreter;

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

            CurrentStatus = new ProgramStatus();
            CurrentStatus.Mode = CpuMode.User;

            InterruptMasterEnable = true;
            EnabledInterrupts = 0;
            AcknowledgedInterrupts = 0;

            ArmInterpreter = new ArmInterpreter(this);
            ThumbInterpreter = new ThumbInterpreter(this);

            MemoryMap = memoryMap;

            // Interrupts MMIO

            memoryMap.RegisterMmio32(0x4000208, () => // IME
            {
                if (InterruptMasterEnable)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }, (x) =>
            {
                InterruptMasterEnable = BitUtil.IsBitSet(x, 0);
            });

            memoryMap.RegisterMmio16(0x4000200, () => // IE
            {
                return (ushort)EnabledInterrupts;
            }, (x) =>
            {
                EnabledInterrupts = x;
            });

            memoryMap.RegisterMmio16(0x4000202, () => // IF
            {
                return (ushort)AcknowledgedInterrupts;
            }, (x) =>
            {
                AcknowledgedInterrupts = x;
            });
        }

        //
        // Helper functions
        //

        private ref uint Reg(int reg)
        {
            return ref CurrentRegisterSet.GetRegister(reg);
        }

        public ref uint RegUser(int reg)
        {
            return ref RegisterSets[CpuMode.User].GetRegister(reg);
        }

        // Step the CPU by one instruction and return the cycles it took to execute
        public int Step()
        {
            if (CurrentStatus.Thumb)
            {
                return ThumbInterpreter.Step();
            }
            else
            {
                return ArmInterpreter.Step();
            }
        }

        public void RaiseInterrupt(InterruptType type)
        {
            if (CurrentStatus.IrqDisable)
            {
                return;
            }

            if (!InterruptMasterEnable)
            {
                return;
            }

            if (!BitUtil.IsBitSet(EnabledInterrupts, (int)type))
            {
                return;
            }

            BitUtil.SetBit(ref AcknowledgedInterrupts, (int)type);

            uint lastPsr = CurrentStatus.RegisterValue;

            CurrentStatus.Mode = CpuMode.Irq;
            CurrentStatus.IrqDisable = true;
            CurrentStatus.Thumb = false;

            CurrentSavedStatus.RegisterValue = lastPsr;

            Reg(LR) = Reg(PC);

            Reg(PC) = 0x00000018; // IRQ vector
        }

    }
}