using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Ram;

namespace AgbSharp.Core.Tests.Memory.Ram
{
    public class InternalWramRegion_Tests : RamRegion_Tests
    {
        protected override uint RegionStart => InternalWramRegion.REGION_START;

        protected override uint RegionSize => InternalWramRegion.REGION_SIZE;

        protected override uint MirrorStart => InternalWramRegion.MIRROR_END + 1 - InternalWramRegion.REGION_SIZE;

        protected override RangedMemoryRegion CreateRegion()
        {
            return new InternalWramRegion();
        }

    }
}