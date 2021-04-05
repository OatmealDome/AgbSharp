using System;
using System.Collections.Generic;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Memory.GamePak;
using Xunit;

namespace AgbSharp.Core.Tests.Memory.GamePak
{
    public class RomRegion_Tests
    {
        private static byte[] CreateDummyData()
        {
            Random random = new Random();

            byte[] buffer = new byte[RomRegion.REGION_SIZE];

            random.NextBytes(buffer);

            return buffer;
        }

        [Theory]
        [MemberData(nameof(TestRanges))]
        public void Read_FromRegion_ReadSuccess(uint startAddress)
        {
            byte[] dummyData = CreateDummyData();

            AgbMemoryMap map = new AgbMemoryMap();
            RomRegion region = new RomRegion(dummyData);

            map.RegisterRegion(region);

            for (uint address = startAddress; address < startAddress + RomRegion.REGION_SIZE; address++)
            {
                Assert.Equal(dummyData[address - startAddress], map.Read(address));
            }
        }

        public static IEnumerable<object[]> TestRanges => new List<object[]>
        {
            new object[] { RomRegion.REGION_ONE_START },
            new object[] { RomRegion.REGION_TWO_START },
            new object[] { RomRegion.REGION_THREE_START }
        };
        
    }
}