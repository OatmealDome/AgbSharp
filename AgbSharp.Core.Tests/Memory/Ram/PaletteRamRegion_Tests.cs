using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Ram;

namespace AgbSharp.Core.Tests.Memory.Ram
{
    public class PaletteRamRegion_Tests : RamRegion_Tests
    {
        protected override uint RegionStart => PaletteRamRegion.REGION_START;

        protected override uint RegionSize => PaletteRamRegion.REGION_SIZE;

        protected override IMemoryRegion CreateRegion()
        {
            return new PaletteRamRegion();
        }

    }
}