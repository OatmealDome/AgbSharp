using AgbSharp.Core.Memory;
using AgbSharp.Core.Ppu;

namespace AgbSharp.Core.Tests.Ppu
{
    class PpuUtil
    {
        public static AgbPpu CreatePpu()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();

            return CreatePpu(memoryMap);
        }

        public static AgbPpu CreatePpu(AgbMemoryMap memoryMap)
        {
            AgbPpu ppu = new AgbPpu(memoryMap);

            return ppu;
        }

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