namespace AgbSharp.Core.Memory.Ram
{
    class OamRegion : RangedMemoryRegion
    {
        public const uint REGION_START = 0x07000000;
        public const uint REGION_SIZE = 0x400;
        public const uint MIRROR_END = 0x07FFFFFF;

        public OamRegion() : base(REGION_START, REGION_SIZE, MIRROR_END)
        {

        }

    }
}