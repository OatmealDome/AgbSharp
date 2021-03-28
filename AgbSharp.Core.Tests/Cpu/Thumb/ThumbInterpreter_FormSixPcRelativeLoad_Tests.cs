using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormSixPcRelativeLoad_Tests
    {
        [Fact]
        public void Load_LoadPcRelativeAddress_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x104;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress - 4, 0xAABBCCDD);
            cpu.MemoryMap.WriteU32(targetAddress, 0xDEADBEEF);
            cpu.MemoryMap.WriteU32(targetAddress + 4, 0xFF112233);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xCAFEBABE;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x4048 // LDR r0, [PC, #0x100]
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
        }

    }
}