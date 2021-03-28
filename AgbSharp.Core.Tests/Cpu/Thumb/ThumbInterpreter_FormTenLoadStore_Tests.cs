using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormTenLoadStore_Tests
    {
        [Fact]
        public void LoadHalf_UsingAddressInRegOneAndImmediateOffset_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x3E;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x8fc8 // LDRH r0, [r1, #0x3E]
            }, true);

            Assert.Equal((uint)0x0000CAFE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void StoreHalf_UsingAddressInRegOneAndImmediateOffset_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x3E;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xF00D;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x87C8 // STRH r0, [r1, #0x3E]
            }, true);

            Assert.Equal((uint)0xF00D, cpu.MemoryMap.ReadU16(targetAddress));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
        }

    }
}