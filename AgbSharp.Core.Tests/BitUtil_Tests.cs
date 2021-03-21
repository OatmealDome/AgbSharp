using AgbSharp.Core.Util;
using System.Collections.Generic;
using Xunit;

namespace AgbSharp.Core.Tests
{
    public class BitUtil_Tests
    {
        [Theory]
        [MemberData(nameof(SequentialBitSet))]
        public void GetBit_VersusSequentialBitSet_One(uint source, int bit)
        {
            Assert.Equal(1, BitUtil.GetBit(source, bit));
        }

        [Theory]
        [MemberData(nameof(SequentialBitSet))]
        public void GetBit_VersusInvertedSequentialBitSet_One(uint source, int bit)
        {
            Assert.Equal(0, BitUtil.GetBit(~source, bit));
        }

        [Theory]
        [MemberData(nameof(SequentialBitSet))]
        public void IsBitSet_VersusSequentialBitSet_True(uint source, int bit)
        {
            Assert.True(BitUtil.IsBitSet(source, bit));
        }

        [Theory]
        [MemberData(nameof(SequentialBitSet))]
        public void IsBitSet_VersusInvertedSequentialBitSet_True(uint source, int bit)
        {
            Assert.False(BitUtil.IsBitSet(~source, bit));
        }

        [Theory]
        [MemberData(nameof(SequentialBitSet))]
        public void SetBit_VersusSequentialBitSet_Equals(uint source, int bit)
        {
            uint i = 0;

            BitUtil.SetBit(ref i, bit);

            Assert.Equal(source, i);
        }

        [Theory]
        [MemberData(nameof(SequentialBitSet))]
        public void ClearBit_VersusSequentialBitSet_Equals(uint source, int bit)
        {
            uint i = 0xFFFFFFFF;

            BitUtil.ClearBit(ref i, bit);

            Assert.Equal(~source, i);
        }

        public static IEnumerable<object[]> SequentialBitSet => new List<object[]>
        {
            new object[] { (uint)0b00000000000000000000000000000001, 0 },
            new object[] { (uint)0b00000000000000000000000000000010, 1 },
            new object[] { (uint)0b00000000000000000000000000000100, 2 },
            new object[] { (uint)0b00000000000000000000000000001000, 3 },
            new object[] { (uint)0b00000000000000000000000000010000, 4 },
            new object[] { (uint)0b00000000000000000000000000100000, 5 },
            new object[] { (uint)0b00000000000000000000000001000000, 6 },
            new object[] { (uint)0b00000000000000000000000010000000, 7 },
            new object[] { (uint)0b00000000000000000000000100000000, 8 },
            new object[] { (uint)0b00000000000000000000001000000000, 9 },
            new object[] { (uint)0b00000000000000000000010000000000, 10 },
            new object[] { (uint)0b00000000000000000000100000000000, 11 },
            new object[] { (uint)0b00000000000000000001000000000000, 12 },
            new object[] { (uint)0b00000000000000000010000000000000, 13 },
            new object[] { (uint)0b00000000000000000100000000000000, 14 },
            new object[] { (uint)0b00000000000000001000000000000000, 15 },
            new object[] { (uint)0b00000000000000010000000000000000, 16 },
            new object[] { (uint)0b00000000000000100000000000000000, 17 },
            new object[] { (uint)0b00000000000001000000000000000000, 18 },
            new object[] { (uint)0b00000000000010000000000000000000, 19 },
            new object[] { (uint)0b00000000000100000000000000000000, 20 },
            new object[] { (uint)0b00000000001000000000000000000000, 21 },
            new object[] { (uint)0b00000000010000000000000000000000, 22 },
            new object[] { (uint)0b00000000100000000000000000000000, 23 },
            new object[] { (uint)0b00000001000000000000000000000000, 24 },
            new object[] { (uint)0b00000010000000000000000000000000, 25 },
            new object[] { (uint)0b00000100000000000000000000000000, 26 },
            new object[] { (uint)0b00001000000000000000000000000000, 27 },
            new object[] { (uint)0b00010000000000000000000000000000, 28 },
            new object[] { (uint)0b00100000000000000000000000000000, 29 },
            new object[] { (uint)0b01000000000000000000000000000000, 30 },
            new object[] { (uint)0b10000000000000000000000000000000, 31 }
        };

    }
}
