namespace AgbSharp.Core.Memory.Ram
{
    class ExternalWramRegion : RangedMemoryRegion
    {
        public const uint REGION_START = 0x02000000;
        public const uint REGION_SIZE = 0x40000;
        public const uint MIRROR_END = 0x02FFFFFF;

        public ExternalWramRegion() : base(REGION_START, REGION_SIZE, MIRROR_END)
        {

        }

    }
}