using AgbSharp.Core.Ppu;

namespace AgbSharp.Core.Tests.Ppu
{
    class PpuUtil
    {
        public static void TickPpuByAmount(AgbPpu ppu, int lines, int dots)
        {
            int totalDots = (lines * 308) + dots;

            for (int i = 0; i < totalDots; i++)
            {
                ppu.Tick();
            }
        }

    }
}