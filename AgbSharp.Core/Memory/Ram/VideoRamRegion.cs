namespace AgbSharp.Core.Memory.Ram
{
    class VideoRamRegion : RangedMemoryRegion
    {
        public const uint REGION_START = 0x06000000;
        public const uint REGION_SIZE = 0x18000;
        public const uint MIRROR_END = 0x06FFFFFF;

        public VideoRamRegion() : base(REGION_START, REGION_SIZE, MIRROR_END)
        {

        }

    }
}