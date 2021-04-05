namespace AgbSharp.Core.Memory.Ram
{
    class PaletteRamRegion : RangedMemoryRegion
    {
        public const uint REGION_START = 0x05000000;
        public const uint REGION_SIZE = 0x400;
        public const uint MIRROR_END = 0x05FFFFFF;

        public PaletteRamRegion() : base(REGION_START, REGION_SIZE, MIRROR_END)
        {

        }

    }
}