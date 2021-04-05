namespace AgbSharp.Core.Memory.Ram
{
    class InternalWramRegion : RangedMemoryRegion
    {
        public const uint REGION_START = 0x3000000;
        public const uint REGION_SIZE = 0x8000;
        public const uint MIRROR_END = 0x03FFFFFF;

        public InternalWramRegion() : base(REGION_START, REGION_SIZE, MIRROR_END)
        {

        }

    }
}