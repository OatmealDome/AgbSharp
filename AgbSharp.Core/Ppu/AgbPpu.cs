using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Interrupt;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Ram;
using AgbSharp.Core.Util;

namespace AgbSharp.Core.Ppu
{
    public class AgbPpu
    {
        private readonly AgbMemoryMap MemoryMap;
        private readonly AgbCpu Cpu;

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

        // Framebuffer in RGBA8 format
        public byte[] Framebuffer;

        // VRAM
        private PaletteRamRegion PaletteRam;
        private VideoRamRegion VideoRam;
        private OamRegion Oam;

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

        public AgbPpu(AgbMemoryMap memoryMap, AgbCpu cpu)
        {
            MemoryMap = memoryMap;
            Cpu = cpu;

            State = PpuState.Render;

            HorizontalDot = 0;
            VerticalLine = 0;

            Framebuffer = new byte[4 * 240 * 160];

            PaletteRam = new PaletteRamRegion();
            memoryMap.RegisterRegion(PaletteRam);

            VideoRam = new VideoRamRegion();
            memoryMap.RegisterRegion(VideoRam);
            
            Oam = new OamRegion();
            memoryMap.RegisterRegion(Oam);
            
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
                Render();
            }

            HorizontalDot++;

            void IncrementVerticalLine()
            {
                HorizontalDot = 0;
                VerticalLine++;

                if (VerticalLine == 228)
                {
                    VerticalLine = 0;
                }

                if (VCountIrqEnable)
                {
                    if (VCountSetting == VerticalLine)
                    {
                        Cpu?.RaiseInterrupt(InterruptType.VCounterMatch);
                    }
                }
            }

            switch (State)
            {
                case PpuState.Render:
                    if (HorizontalDot == 240)
                    {
                        State = PpuState.HBlank;

                        if (HBlankIrqEnable)
                        {
                            Cpu?.RaiseInterrupt(InterruptType.HBlank);
                        }
                    }

                    break;
                case PpuState.HBlank:
                    if (HorizontalDot == 308)
                    {
                        IncrementVerticalLine();

                        State = PpuState.Render;
                    }

                    if (VerticalLine == 160)
                    {
                        State = PpuState.VBlank;

                        if (VBlankIrqEnable)
                        {
                            Cpu?.RaiseInterrupt(InterruptType.VBlank);
                        }
                    }

                    break;
                case PpuState.VBlank:
                    if (HorizontalDot == 308)
                    {
                        IncrementVerticalLine();
                    }

                    if (VerticalLine == 0)
                    {
                        State = PpuState.Render;
                    }

                    break;
            }
        }

        // Colour intensities are 0 to 31
        private void DrawDot(int r, int g, int b)
        {
            int outputOfs = ((VerticalLine * 240) + HorizontalDot) * 4;

            byte GbaColourToOutputColour(int colour)
            {
                return (byte)((colour / 31.0f) * 255.0f);
            }

            Framebuffer[outputOfs] = GbaColourToOutputColour(r);
            Framebuffer[outputOfs + 1] = GbaColourToOutputColour(g);
            Framebuffer[outputOfs + 2] = GbaColourToOutputColour(b);
            Framebuffer[outputOfs + 3] = 0xFF; // opaque
        }

        private void Render()
        {
            if (ForcedBlank)
            {
                // During forced blank, the PPU always renders white
                DrawDot(31, 31, 31);

                return;
            }

            switch (BackgroundMode)
            {
                default:
                    // Unimplemented modes will appear as an all-blue screen
                    DrawDot(0, 0, 31);
                    
                    return;
            }

        }

    }
}