using AgbSharp.Core.Util;
using Xunit;

namespace AgbSharp.Core.Tests.Util
{
    public class ByteUtil_Tests
    {
        [Fact]
        public void SwapEndianness_SwapConstant_EqualExpected()
        {
            uint i = 0xAABBCCDD;

            Assert.Equal((uint)0xDDCCBBAA, ByteUtil.SwapEndianness(i));
        }

    }
}