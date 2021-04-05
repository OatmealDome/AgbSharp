using System;
using System.Collections.Generic;
using AgbSharp.Core.Memory;
using Xunit;

namespace AgbSharp.Core.Tests.Memory
{
    public class AgbMemoryMap_Tests
    {
        private const uint TEST_REGION_SIZE = 0x1000;
        private const uint FIRST_RANGE_START = 0x0;
        private const uint SECOND_RANGE_START = 0x10000;

        class TestMemoryRegion : RangedMemoryRegion
        {
            private byte[] Bytes;

            public TestMemoryRegion() : base(FIRST_RANGE_START, TEST_REGION_SIZE, SECOND_RANGE_START + TEST_REGION_SIZE)
            {
                Bytes = new byte[TEST_REGION_SIZE];
            }
            
        }

        [Theory]
        [MemberData(nameof(TestRanges))]
        public void Read_RegisteredTestRegion_ReadRangeSuccess(uint rangeStart)
        {
            AgbMemoryMap map = new AgbMemoryMap();
            RangedMemoryRegion region = new TestMemoryRegion();

            // Write direct to the region instead of through AgbMemoryMap
            for (uint i = rangeStart; i < rangeStart + TEST_REGION_SIZE; i++)
            {
                region.Write(i, (byte)(i & 0xFF));
            }

            map.RegisterRegion(region);

            for (uint i = rangeStart; i < rangeStart + TEST_REGION_SIZE; i++)
            {
                Assert.Equal(i & 0xFF, map.Read(i));
            }
        }

        [Theory]
        [MemberData(nameof(TestRanges))]
        public void Write_RegisteredTestRegion_WriteRangeSuccess(uint rangeStart)
        {
            AgbMemoryMap map = new AgbMemoryMap();
            RangedMemoryRegion region = new TestMemoryRegion();

            map.RegisterRegion(region);

            for (uint i = rangeStart; i < rangeStart + TEST_REGION_SIZE; i++)
            {
                byte value = (byte)(i & 0xFF);

                map.Write(i, value);

                Assert.Equal(value, map.Read(i));
            }
        }

        [Fact]
        public void ReadU32_WithDummyData_ReadSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            RangedMemoryRegion region = new TestMemoryRegion();

            // Write directly to the region
            region.Write(FIRST_RANGE_START, 0xDD);
            region.Write(FIRST_RANGE_START + 1, 0xCC);
            region.Write(FIRST_RANGE_START + 2, 0xBB);
            region.Write(FIRST_RANGE_START + 3, 0xAA);

            map.RegisterRegion(region);

            Assert.Equal(0xAABBCCDD, map.ReadU32(FIRST_RANGE_START));
        }

        [Fact]
        public void WriteU32_WithDummyData_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            RangedMemoryRegion region = new TestMemoryRegion();

            map.RegisterRegion(region);

            map.WriteU32(FIRST_RANGE_START, 0xAABBCCDD);

            Assert.Equal(0xAABBCCDD, map.ReadU32(FIRST_RANGE_START));
        }

        [Fact]
        public void ReadU16_WithDummyData_ReadSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            RangedMemoryRegion region = new TestMemoryRegion();

            // Write directly to the region
            region.Write(FIRST_RANGE_START, 0xBB);
            region.Write(FIRST_RANGE_START + 1, 0xAA);

            map.RegisterRegion(region);

            Assert.Equal(0xAABB, map.ReadU16(FIRST_RANGE_START));
        }

        [Fact]
        public void WriteU16_WithDummyData_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            RangedMemoryRegion region = new TestMemoryRegion();

            map.RegisterRegion(region);

            map.WriteU16(FIRST_RANGE_START, 0xAABB);

            Assert.Equal(0xAABB, map.ReadU16(FIRST_RANGE_START));
        }

        public static IEnumerable<object[]> TestRanges => new List<object[]>
        {
            new object[] { FIRST_RANGE_START },
            new object[] { SECOND_RANGE_START }
        };
        
    }
}