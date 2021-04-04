namespace AgbSharp.Core.Ppu
{
    class AgbPpu
    {
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

        public AgbPpu()
        {
            State = PpuState.Render;   
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
                    if (HorizontalDot == 68)
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