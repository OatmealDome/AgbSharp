using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AgbSharp.Core.Tests")]
namespace AgbSharp.Core.Util
{
    internal class BitUtil
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBit(uint b, int bit)
        {
            return (int)(b >> bit) & 0x1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBitRange(uint b, int start, int end)
        {
            int mask = (1 << (end - start + 1)) - 1;

            return (int)(b >> start) & mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBitSet(uint b, int bit)
        {
            return GetBit(b, bit) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBit(ref uint b, int bit)
        {
            // for example, setting Carry on GbSharp CpuFlags:
            // F    1000 0000
            // mask 0001 0000
            // XOR  1001 0000
            b |= (uint)(1 << bit);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ClearBit(ref uint b, int bit)
        {
            // for example, clearing Carry on GbSharp CpuFlags:
            // F      1001 0000
            // mask   0001 0000
            // invert 1110 0000
            // AND    1000 0000
            b &= (uint)~(1 << bit);
        }

    }
}
