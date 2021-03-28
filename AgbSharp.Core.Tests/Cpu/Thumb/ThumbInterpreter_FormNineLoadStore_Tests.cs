using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormNineLoadStore_Tests
    {
        [Fact]
        public void Load_UsingAddressInRegOneAndImmediateOffset_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x7C;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x6fc8 // LDR r0, [r1, #0x7C]
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void LoadByte_UsingAddressInRegOneAndImmediateOffset_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1F;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.Write(targetAddress, 0xAA);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x7FC8 // LDRB r0, [r1, #0x1F]
            }, true);

            Assert.Equal((uint)0x000000AA, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Store_UsingAddressInRegOneAndImmediateOffset_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x7C;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x67C8 // STR r0, [r1, #0x7C]
            }, true);

            Assert.Equal(0xDEADBEEF, cpu.MemoryMap.ReadU32(targetAddress));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void StoreByte_UsingAddressInRegOneAndImmediateOffset_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1F;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xFEFEFEAA;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x77c8 // STRB r0, [r1, #0x1F]
            }, true);

            Assert.Equal(0xAA, cpu.MemoryMap.Read(targetAddress));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
        }
        
    }
}