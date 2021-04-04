using AgbSharp.Core.Ppu;
using Xunit;

namespace AgbSharp.Core.Tests.Ppu
{
    // Might contain a lot of superfluous tests, but I want to avoid the
    // situation with GbSharp where each frame had too many lines and dots
    // because of an off-by-one error...
    public class AgbPpu_Timing_Tests
    {
        [Fact]
        public void Tick_Tick239Dots_InRender()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 0, 239);

            Assert.Equal(PpuState.Render, ppu.State);
        }

        [Fact]
        public void Tick_Tick240Dots_InHBlank()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 0, 240);

            Assert.Equal(PpuState.HBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick307Dots_InHBlank()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 0, 307);

            Assert.Equal(PpuState.HBlank, ppu.State);
        }

        [Fact]
        public void Tick_TickOneLine_InRender()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 1, 0);

            Assert.Equal(PpuState.Render, ppu.State);
        }

        [Fact]
        public void Tick_Tick159Lines_InRender()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 159, 0);

            Assert.Equal(PpuState.Render, ppu.State);
        }

        [Fact]
        public void Tick_Tick159LinesAnd240Dots_InHBlank()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 159, 240);

            Assert.Equal(PpuState.HBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick160Lines_InVBlank()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 160, 0);

            Assert.Equal(PpuState.VBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick160LinesAnd240Dots_InVBlank()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 160, 240);

            Assert.Equal(PpuState.VBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick227Lines_InVBlank()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 227, 0);

            Assert.Equal(PpuState.VBlank, ppu.State);
        }

        [Fact]
        public void Tick_Tick229Lines_InRender()
        {
            AgbPpu ppu = PpuUtil.CreatePpu();

            PpuUtil.TickPpuByAmount(ppu, 229, 0);

            Assert.Equal(PpuState.Render, ppu.State);
        }

    }
}