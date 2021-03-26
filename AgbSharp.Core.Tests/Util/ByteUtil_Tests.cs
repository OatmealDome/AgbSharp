using AgbSharp.Core.Util;
using Xunit;

namespace AgbSharp.Core.Tests.Util
{
    public class ByteUtil_Tests
    {
        [Fact]
        public void Swap32_SwapConstant_EqualExpected()
        {
            uint i = 0xAABBCCDD;

            Assert.Equal((uint)0xDDCCBBAA, ByteUtil.Swap32(i));
        }

    }
}