using System.Runtime.CompilerServices;

namespace AgbSharp.Core.Util
{
    public class ByteUtil
    {
        // adapted from https://stackoverflow.com/a/8241150
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Swap32(uint value)
        {
            var b1 = value & 0xff;
            var b2 = (value >> 8) & 0xff;
            var b3 = (value >> 16) & 0xff;
            var b4 = value >> 24;

            return b1 << 24 | b2 << 16 | b3 << 8 | b4;
        }

    }
}