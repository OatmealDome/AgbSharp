using AgbSharp.Core.Util;

namespace AgbSharp.Core.Cpu.Status
{
    // CPSR and SPSR
    public class ProgramStatus
    {
        public bool Negative
        {
            get;
            set;
        }

        public bool Zero
        {
            get;
            set;
        }

        public bool Carry
        {
            get;
            set;
        }

        public bool Overflow
        {
            get;
            set;
        }

        // ARMv5 and up only
        public bool StickyOverflow
        {
            get;
            set;
        }

        public bool IrqDisable
        {
            get;
            set;
        }

        public bool FastIrqDisable
        {
            get;
            set;
        }

        public bool Thumb
        {
            get;
            set;
        }

        public CpuMode Mode
        {
            get;
            set;
        }

        public uint RegisterValue
        {
            get
            {
                uint i = 0;

                if (Negative)
                {
                    BitUtil.SetBit(ref i, 31);
                }

                if (Zero)
                {
                    BitUtil.SetBit(ref i, 30);
                }

                if (Carry)
                {
                    BitUtil.SetBit(ref i, 29);
                }

                if (Overflow)
                {
                    BitUtil.SetBit(ref i, 28);
                }

                if (StickyOverflow)
                {
                    BitUtil.SetBit(ref i, 27);
                }

                // Bits 8 to 26 are reserved

                if (IrqDisable)
                {
                    BitUtil.SetBit(ref i, 7);
                }

                if (FastIrqDisable)
                {
                    BitUtil.SetBit(ref i, 6);
                }

                if (Thumb)
                {
                    BitUtil.SetBit(ref i, 5);
                }                

                // CPU mode is bits 0 to 4, so just OR it
                i |= (uint)Mode;

                return i;
            }
            set
            {
                Negative = BitUtil.IsBitSet(value, 31);
                Zero = BitUtil.IsBitSet(value, 30);
                Carry = BitUtil.IsBitSet(value, 29);
                Overflow = BitUtil.IsBitSet(value, 28);
                StickyOverflow = BitUtil.IsBitSet(value, 27);
                IrqDisable = BitUtil.IsBitSet(value, 7);
                FastIrqDisable = BitUtil.IsBitSet(value, 6);
                Thumb = BitUtil.IsBitSet(value, 5);
                Mode = (CpuMode)(value & 0x1f);
            }
        }

    }
}