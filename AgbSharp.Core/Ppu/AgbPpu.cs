using AgbSharp.Core.Memory;
using AgbSharp.Core.Util;

namespace AgbSharp.Core.Ppu
{
    class AgbPpu
    {
        private readonly AgbMemoryMap MemoryMap;

        //
        // AGB PPU timing (from GBATEK):
        //
        // 1 dot per 4 CPU cycles
        //
        // Horizontal:
        // 240 dots visible
        //  68 dots H-Blank
        // ----------------
        // 308 dots total
        //
        // Vertical:
        // 160 lines visible
        //  68 lines V-Blank
        // -----------------
        // 228 lines total
        //

        public PpuState State
        {
            get;
            private set;
        }

        private int HorizontalDot;
        private int VerticalLine;

        // DISPCNT
        private int BackgroundMode;
        private bool CgbMode; // always false for AGB
        private bool UseAltFrame; // BG Modes 4 and 5 have two framebuffers
        private bool AllowOamDuringHBlank;
        private bool UseOneDimensionalObjs;
        private bool ForcedBlank;
        private bool DisplayBgZero;
        private bool DisplayBgOne;
        private bool DisplayBgTwo;
        private bool DisplayBgThree;
        private bool DisplayObjs;
        private bool DisplayWindowZero;
        private bool DisplayWindowOne;
        private bool DisplayWindowObjs;

        // DISPSTAT
        private bool VBlankIrqEnable;
        private bool HBlankIrqEnable;
        private bool VCountIrqEnable;
        private int VCountSetting;

        public AgbPpu(AgbMemoryMap memoryMap)
        {
            MemoryMap = memoryMap;

            State = PpuState.Render;

            HorizontalDot = 0;
            VerticalLine = 0;

            #region DISPCNT

            BackgroundMode = 0;
            CgbMode = false;
            UseAltFrame = false;
            AllowOamDuringHBlank = false;
            UseOneDimensionalObjs = false;
            ForcedBlank = false;
            DisplayBgZero = false;
            DisplayBgOne = false;
            DisplayBgTwo = false;
            DisplayBgThree = false;
            DisplayObjs = false;
            DisplayWindowZero = false;
            DisplayWindowOne = false;
            DisplayWindowObjs = false;

            MemoryMap.RegisterMmio16(0x4000000, () =>
            {
                ushort x = 0;

                if (CgbMode)
                {
                    BitUtil.SetBit(ref x, 3);
                }

                if (UseAltFrame)
                {
                    BitUtil.SetBit(ref x, 4);
                }

                if (AllowOamDuringHBlank)
                {
                    BitUtil.SetBit(ref x, 5);
                }

                if (UseOneDimensionalObjs)
                {
                    BitUtil.SetBit(ref x, 6);
                }

                if (ForcedBlank)
                {
                    BitUtil.SetBit(ref x, 7);
                }

                if (DisplayBgZero)
                {
                    BitUtil.SetBit(ref x, 8);
                }

                if (DisplayBgOne)
                {
                    BitUtil.SetBit(ref x, 9);
                }

                if (DisplayBgTwo)
                {
                    BitUtil.SetBit(ref x, 10);
                }

                if (DisplayBgThree)
                {
                    BitUtil.SetBit(ref x, 11);
                }

                if (DisplayObjs)
                {
                    BitUtil.SetBit(ref x, 12);
                }

                if (DisplayWindowZero)
                {
                    BitUtil.SetBit(ref x, 13);
                }

                if (DisplayWindowOne)
                {
                    BitUtil.SetBit(ref x, 14);
                }

                if (DisplayWindowObjs)
                {
                    BitUtil.SetBit(ref x, 15);
                }

                x |= (ushort)BackgroundMode;

                return x;
            }, (x) =>
            {
                BackgroundMode = BitUtil.GetBitRange(x, 0, 2);
                CgbMode = BitUtil.IsBitSet(x, 3);
                UseAltFrame = BitUtil.IsBitSet(x, 4);
                AllowOamDuringHBlank = BitUtil.IsBitSet(x, 5);
                UseOneDimensionalObjs = BitUtil.IsBitSet(x, 6);
                ForcedBlank = BitUtil.IsBitSet(x, 7);
                DisplayBgZero = BitUtil.IsBitSet(x, 8);
                DisplayBgOne = BitUtil.IsBitSet(x, 9);
                DisplayBgTwo = BitUtil.IsBitSet(x, 10);
                DisplayBgThree = BitUtil.IsBitSet(x, 11);
                DisplayObjs = BitUtil.IsBitSet(x, 12);
                DisplayWindowZero = BitUtil.IsBitSet(x, 13);
                DisplayWindowOne = BitUtil.IsBitSet(x, 14);
                DisplayWindowObjs = BitUtil.IsBitSet(x, 15);
            });

            #endregion
        
            #region DISPSTAT

            VBlankIrqEnable = false;
            HBlankIrqEnable = false;
            VCountIrqEnable = false;
            VCountSetting = 0;

            MemoryMap.RegisterMmio16(0x4000004, () =>
            {
                ushort x = 0;

                if (State == PpuState.VBlank && VerticalLine != 227)
                {
                    BitUtil.SetBit(ref x, 0);
                }

                if (HorizontalDot >= 240) // H-Blank flag set even in V-Blank
                {
                    BitUtil.SetBit(ref x, 1);
                }

                if (VCountSetting == VerticalLine)
                {
                    BitUtil.SetBit(ref x, 2);
                }

                if (VBlankIrqEnable)
                {
                    BitUtil.SetBit(ref x, 3);
                }

                if (HBlankIrqEnable)
                {
                    BitUtil.SetBit(ref x, 4);
                }

                if (VCountIrqEnable)
                {
                    BitUtil.SetBit(ref x, 5);
                }

                // Bits 6 and 7 unused on AGB

                x |= (ushort)(VCountSetting << 8);

                return x;
            }, (x) =>
            {
                // Bits 0 to 2 are read-only, and bits 6 and 7 are unused on AGB
                VBlankIrqEnable = BitUtil.IsBitSet(x, 3);
                HBlankIrqEnable = BitUtil.IsBitSet(x, 4);
                VCountIrqEnable = BitUtil.IsBitSet(x, 5);
                VCountSetting = BitUtil.GetBitRange(x, 8, 15);
            });

            #endregion
        
            #region VCOUNT

            MemoryMap.RegisterMmio16(0x4000006, () =>
            {
                // Bit 8 unused on AGB, bits 9 to 15 unused on all platforms
                
                return (ushort)VerticalLine;
            }, (x) =>
            {
                // Writes ignored
            });

            #endregion
        }

        public void Tick()
        {
            if (State == PpuState.Render)
            {
                // TODO: Rendering
            }

            HorizontalDot++;

            switch (State)
            {
                case PpuState.Render:
                    if (HorizontalDot == 240)
                    {
                        State = PpuState.HBlank;
                    }

                    break;
                case PpuState.HBlank:
                    if (HorizontalDot == 308)
                    {
                        HorizontalDot = 0;
                        VerticalLine++;

                        State = PpuState.Render;
                    }

                    if (VerticalLine == 160)
                    {
                        State = PpuState.VBlank;
                    }

                    break;
                case PpuState.VBlank:
                    if (HorizontalDot == 308)
                    {
                        HorizontalDot = 0;
                        VerticalLine++;
                    }

                    if (VerticalLine == 228)
                    {
                        State = PpuState.Render;
                    }

                    break;
            }
        }

    }
}