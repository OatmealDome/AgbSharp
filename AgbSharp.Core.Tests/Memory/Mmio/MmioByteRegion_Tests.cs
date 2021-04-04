using AgbSharp.Core.Memory;
using Xunit;

namespace AgbSharp.Core.Tests.Memory.Mmio
{
    public class MmioByteRegion_Tests
    {
        [Fact]
        public void Read_RegisteredMmio_ReadSuccess()
        {
            AgbMemoryMap map = new AgbMemoryMap();
            
            map.RegisterMmio(0xDEADBEEF, () => 0xAA, (b) => { });

            map.UpdateMmio();

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

            map.FlushMmio();

            Assert.Equal(0xBB, val);
        }
        
    }
}