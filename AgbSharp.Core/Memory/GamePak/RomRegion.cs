using System.Collections.Generic;

namespace AgbSharp.Core.Memory.GamePak
{
    class RomRegion : RangedMemoryRegion
    {
        public const uint REGION_ONE_START = 0x08000000;
        public const uint REGION_TWO_START = 0x0A000000;
        public const uint REGION_THREE_START = 0x0C000000;
        public const uint REGION_SIZE = 0x2000000;

        public RomRegion(byte[] data) : base(REGION_ONE_START, REGION_SIZE, REGION_THREE_START + REGION_SIZE - 1, data)
        {

        }

        public override IEnumerable<byte> GetHandledRanges()
        {
            // We can stretch across multiple "ranges".
            return new List<byte>()
            {
                0x08,
                0x09,
                0x0A,
                0x0B,
                0x0C,
                0x0D
            };
        }

        public override void Write(uint address, byte val)
        {
            // Can't write to ROM
        }

    }
}
