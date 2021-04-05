namespace AgbSharp.Core.Memory.Bios
{
    class BiosRegion : RangedMemoryRegion
    {
        public const uint REGION_START = 0x00000000;
        public const uint REGION_SIZE = 0x4000;

        private byte[] Data;

        public BiosRegion(byte[] data) : base(REGION_START, REGION_SIZE, REGION_START + REGION_SIZE, data)
        {
            Data = data;
        }

        public override void Write(uint address, byte val)
        {
            // Can't write to BIOS
        }
        
    }
}
