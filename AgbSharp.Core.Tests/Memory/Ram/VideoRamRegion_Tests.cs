using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.Ram;

namespace AgbSharp.Core.Tests.Memory.Ram
{
    public class VideoRamRegion_Tests : RamRegion_Tests
    {
        protected override uint RegionStart => VideoRamRegion.REGION_START;

        protected override uint RegionSize => VideoRamRegion.REGION_SIZE;

        protected override uint MirrorStart => VideoRamRegion.MIRROR_END + 1 - VideoRamRegion.REGION_SIZE;

        protected override RangedMemoryRegion CreateRegion()
        {
            return new VideoRamRegion();
        }

    }
}