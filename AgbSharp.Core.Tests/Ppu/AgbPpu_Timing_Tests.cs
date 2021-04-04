using AgbSharp.Core.Ppu;
using Xunit;

namespace AgbSharp.Core.Tests.Ppu
{
    // Might contain a lot of superfluous tests, but I want to avoid the
    // situation with GbSharp where each frame had too many lines and dots
    // because of an off-by-one error...
    public class AgbPpu_Timing_Tests
    {
        private static void TickPpuByAmount(AgbPpu ppu, int lines, int dots)
        {
            int totalDots = (lines * 308) + dots;

            for (int i = 0; i < totalDots; i++)
            {
                ppu.Tick();
            }
        }

        [Fact]
        public void Tick_Tick67Dots_InRender()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 0, 67);

            Assert.Equal(PpuState.Render, ppu.State);
        }

        [Fact]
        public void Tick_Tick68Dots_InHBlank()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 0, 68);

            Assert.Equal(PpuState.HBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick307Dots_InHBlank()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 0, 307);

            Assert.Equal(PpuState.HBlank, ppu.State);
        }

        [Fact]
        public void Tick_TickOneLine_InRender()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 1, 0);

            Assert.Equal(PpuState.Render, ppu.State);
        }

        [Fact]
        public void Tick_Tick159Lines_InRender()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 159, 0);

            Assert.Equal(PpuState.Render, ppu.State);
        }

        [Fact]
        public void Tick_Tick159LinesAnd68Dots_InHBlank()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 159, 68);

            Assert.Equal(PpuState.HBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick160Lines_InVBlank()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 160, 0);

            Assert.Equal(PpuState.VBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick160LinesAnd68Dots_InVBlank()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 160, 68);

            Assert.Equal(PpuState.VBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick227Lines_InVBlank()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 227, 0);

            Assert.Equal(PpuState.VBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick229Lines_InRender()
        {
            AgbPpu ppu = new AgbPpu();

            TickPpuByAmount(ppu, 229, 0);

            Assert.Equal(PpuState.Render, ppu.State);
        }

    }
}