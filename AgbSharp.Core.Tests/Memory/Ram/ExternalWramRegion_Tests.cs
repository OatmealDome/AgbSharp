using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Ram;

namespace AgbSharp.Core.Tests.Memory.Ram
{
    public class ExternalWramRegion_Tests : RamRegion_Tests
    {
        protected override uint RegionStart => ExternalWramRegion.REGION_START;

        protected override uint RegionSize => ExternalWramRegion.REGION_SIZE;

        protected override IMemoryRegion CreateRegion()
        {
            return new ExternalWramRegion();
        }

    }
}