using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Interrupt;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Util;

namespace AgbSharp.Core.Controller
{
    public class AgbController
    {
        private AgbCpu Cpu;
        
        private uint PressedKeys;
        private uint InterruptBitfield;
        private bool InterruptsEnabled;
        private ControllerInterruptCondition InterruptCondition;

        public AgbController(AgbMemoryMap memoryMap, AgbCpu cpu)
        {
            Cpu = cpu;

            PressedKeys = 0x3FF; // all released
            InterruptBitfield = 0; // all ignore
            InterruptCondition = 0; // disabled
            InterruptCondition = ControllerInterruptCondition.LogicalOr;

            memoryMap.RegisterMmio16(0x4000130, () =>
            {
                return (ushort)PressedKeys;
            }, (x) =>
            {
                // Reads ignored
            });

            memoryMap.RegisterMmio16(0x4000132, () =>
            {
                ushort x = (ushort)InterruptBitfield;

                if (InterruptsEnabled)
                {
                    BitUtil.SetBit(ref x, 14);
                }

                if (InterruptCondition == ControllerInterruptCondition.LogicalAnd)
                {
                    BitUtil.SetBit(ref x, 15);
                }

                return x;
            }, (x) =>
            {
                InterruptBitfield = (uint)(x & 0x3ff);
                InterruptsEnabled = BitUtil.IsBitSet(x, 14);

                if (BitUtil.IsBitSet(x, 15))
                {
                    InterruptCondition = ControllerInterruptCondition.LogicalAnd;
                }
                else
                {
                    InterruptCondition = ControllerInterruptCondition.LogicalOr;
                }
            });
        }

        public void UpdateKeyState(ControllerKey controllerKey, bool state)
        {
            // This is correct - key states are inverted in the MMIO
            // 1 = not pressed, 0 = pressed
            if (state)
            {
                BitUtil.ClearBit(ref PressedKeys, (int)controllerKey);
            }
            else
            {
                BitUtil.SetBit(ref PressedKeys, (int)controllerKey);
            }

            // Check if we should raise an interrupt
            if (InterruptsEnabled)
            {
                bool condition;
                if (InterruptCondition == ControllerInterruptCondition.LogicalOr)
                {
                    condition = (~PressedKeys & InterruptBitfield) != 0;   
                }
                else // AND
                {
                    condition = (~PressedKeys & InterruptBitfield) == InterruptBitfield;
                }

                if (condition)
                {
                    Cpu.RaiseInterrupt(InterruptType.Key);
                }
            }
        }

    }
}