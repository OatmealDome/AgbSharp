using AgbSharp.Core.Memory;
using Xunit;

namespace AgbSharp.Core.Tests.Memory.Mmio
{
    public class MmioWordRegion_Tests
    {
        [Fact]
        public void Read_RegisteredMmio32_ReadSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            
            map.RegisterMmio32(0xDEADBEEF, () => 0xAABBCCDD, (b) => { });

            map.UpdateMmio();

            Assert.Equal(0xAABBCCDD, map.ReadU32(0xDEADBEEF));
        }

        [Fact]
        public void Write_RegisteredMmio32AndWriteToFirstByte_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            uint val = 0xCAFEBABE;
            
            map.RegisterMmio32(0xDEADBEEF, () => val, (b) =>
            {
                val = b;
            });

            map.Write(0xDEADBEEF, 0xAA);

            map.FlushMmio();

            Assert.Equal(0xCAFEBAAA, val);
        }

        [Fact]
        public void Write_RegisteredMmio32AndWriteToSecondByte_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            uint val = 0xCAFEBABE;
            
            map.RegisterMmio32(0xDEADBEEF, () => val, (b) =>
            {
                val = b;
            });

            map.Write(0xDEADBEEF + 1, 0xBB);

            map.FlushMmio();

            Assert.Equal(0xCAFEBBBE, val);
        }

        [Fact]
        public void Write_RegisteredMmio32AndWriteToThirdByte_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            uint val = 0xCAFEBABE;
            
            map.RegisterMmio32(0xDEADBEEF, () => val, (b) =>
            {
                val = b;
            });

            map.Write(0xDEADBEEF + 2, 0xCC);

            map.FlushMmio();

            Assert.Equal(0xCACCBABE, val);
        }

        [Fact]
        public void Write_RegisteredMmio32AndWriteToFourthByte_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            uint val = 0xCAFEBABE;
            
            map.RegisterMmio32(0xDEADBEEF, () => val, (b) =>
            {
                val = b;
            });

            map.Write(0xDEADBEEF + 3, 0xDD);

            map.FlushMmio();

            Assert.Equal(0xDDFEBABE, val);
        }

        [Fact]
        public void Write_RegisteredMmio32AndWriteToAll_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            uint val = 0xCAFEBABE;
            
            map.RegisterMmio32(0xDEADBEEF, () => val, (b) =>
            {
                val = b;
            });

            map.WriteU32(0xDEADBEEF, 0xAABBCCDD);

            map.FlushMmio();

            Assert.Equal(0xAABBCCDD, val);
        }
        
    }
}