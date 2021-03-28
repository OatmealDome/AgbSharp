using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormElevenLoadStore_Tests
    {
        [Fact]
        public void Load_UsingAddressInSpAndImmediateOffset_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x3FC;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(CpuUtil.SP) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x98FF // LDR r0, [SP, #0x3FC]
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(CpuUtil.SP));
        }

        [Fact]
        public void Store_UsingAddressInSpAndImmediateOffset_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x3FC;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(CpuUtil.SP) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x90FF // STR r0, [SP, #0x3FC]
            }, true);

            Assert.Equal(0xDEADBEEF, cpu.MemoryMap.ReadU32(targetAddress));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(CpuUtil.SP));
        }

    }
}