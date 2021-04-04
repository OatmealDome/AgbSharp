using AgbSharp.Core.Memory;
using Xunit;

namespace AgbSharp.Core.Tests.Memory.Mmio
{
    public class MmioHalfWordRegion_Tests
    {
        [Fact]
        public void Read_RegisteredMmio16_ReadSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            
            map.RegisterMmio16(0xDEADBEEF, () => 0xAABB, (b) => { });

            map.UpdateMmio();

            Assert.Equal(0xAABB, map.ReadU16(0xDEADBEEF));
        }

        [Fact]
        public void Write_RegisteredMmio16AndWriteToHi_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            ushort val = 0xFFFE;
            
            map.RegisterMmio16(0xDEADBEEF, () => val, (b) =>
            {
                val = b;
            });

            map.Write(0xDEADBEEF, 0xBB);

            map.FlushMmio();

            Assert.Equal(0xFFBB, val);
        }

        [Fact]
        public void Write_RegisteredMmio16AndWriteToLo_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            ushort val = 0xFFFE;
            
            map.RegisterMmio16(0xDEADBEEF, () => val, (b) =>
            {
                val = b;
            });

            map.Write(0xDEADBEEF + 1, 0xAA);

            map.FlushMmio();

            Assert.Equal(0xAAFE, val);
        }

        [Fact]
        public void Write_RegisteredMmio16AndWriteToHiAndLo_WriteSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            ushort val = 0xFFFE;
            
            map.RegisterMmio16(0xDEADBEEF, () => val, (b) =>
            {
                val = b;
            });

            map.WriteU16(0xDEADBEEF, 0xAABB);

            map.FlushMmio();

            Assert.Equal(0xAABB, val);
        }
        
    }
}