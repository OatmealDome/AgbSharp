using System;
using System.Collections.Generic;
using AgbSharp.Core.Memory;
using Xunit;

namespace AgbSharp.Core.Tests
{
    public class AgbMemoryMap_Tests
    {
        private const uint TEST_REGION_SIZE = 0x1000;
        private const uint FIRST_RANGE_START = 0x0;
        private const uint SECOND_RANGE_START = 0x10000;

        class TestMemoryRegion : IMemoryRegion
        {
            private byte[] Bytes;

            public TestMemoryRegion()
            {
                Bytes = new byte[TEST_REGION_SIZE];
            }

            public IEnumerable<Tuple<uint, uint>> GetHandledRanges()
            {
                return new List<Tuple<uint, uint>>()
                {
                    new Tuple<uint, uint>(FIRST_RANGE_START, TEST_REGION_SIZE),
                    new Tuple<uint, uint>(SECOND_RANGE_START, TEST_REGION_SIZE)
                };
            }

            public byte Read(uint address)
            {
                if (address >= SECOND_RANGE_START)
                {
                    address -= SECOND_RANGE_START;
                }

                return Bytes[address];
            }

            public void Write(uint address, byte val)
            {
                if (address >= SECOND_RANGE_START)
                {
                    address -= SECOND_RANGE_START;
                }
                
                Bytes[address] = val;
            }
        }

        [Theory]
        [MemberData(nameof(TestRanges))]
        public void Read_RegisteredTestRegion_ReadRangeSuccess(uint rangeStart)
        {
            AgbMemoryMap map = new AgbMemoryMap();
            IMemoryRegion region = new TestMemoryRegion();

            // Write direct to the region instead of through AgbMemoryMap
            for (uint i = rangeStart; i < TEST_REGION_SIZE; i++)
            {
                region.Write(i, (byte)(i & 0xFF));
            }

            map.RegisterRegion(region);

            for (uint i = rangeStart; i < TEST_REGION_SIZE; i++)
            {
                Assert.Equal(i & 0xFF, map.Read(i));
            }
        }

        [Theory]
        [MemberData(nameof(TestRanges))]
        public void Write_RegisteredTestRegion_WriteRangeSuccess(uint rangeStart)
        {
            AgbMemoryMap map = new AgbMemoryMap();
            IMemoryRegion region = new TestMemoryRegion();

            map.RegisterRegion(region);

            for (uint i = rangeStart; i < TEST_REGION_SIZE; i++)
            {
                byte value = (byte)(i & 0xFF);

                map.Write(i, value);

                Assert.Equal(value, map.Read(i));
            }
        }

        [Fact]
        public void Read_RegisteredMmio_ReadSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            
            map.RegisterMmio(0xDEADBEEF, () => 0xAA, (b) => { });

            Assert.Equal(0xAA, map.Read(0xDEADBEEF));
        }

        [Fact]
        public void Write_RegisteredMmio_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            byte val = 0;
            
            map.RegisterMmio(0xDEADBEEF, () => val, (b) =>
            {
                val = b;
            });

            map.Write(0xDEADBEEF, 0xBB);

            Assert.Equal(0xBB, val);
        }

        public static IEnumerable<object[]> TestRanges => new List<object[]>
        {
            new object[] { FIRST_RANGE_START },
            new object[] { SECOND_RANGE_START }
        };
        
    }
}