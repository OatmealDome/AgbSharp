using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormSevenLoadStore_Tests
    {
        [Fact]
        public void Load_UsingAddressInRegOneAndOffsetInRegTwo_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x5888 // LDR r0, [r1, r2]
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void LoadByte_UsingAddressInRegOneAndOffsetInRegTwo_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.Write(targetAddress, 0xAA);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x5c88 // LDRB r0, [r1, r2]
            }, true);

            Assert.Equal((uint)0x000000AA, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Store_UsingAddressInRegOneAndOffsetInRegTwo_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x5088 // STR r0, [r1, r2]
            }, true);

            Assert.Equal(0xDEADBEEF, cpu.MemoryMap.ReadU32(targetAddress));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void StoreByte_UsingAddressInRegOneAndOffsetInRegTwo_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xFEFEFEAA;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x5488 // STRB r0, [r1, r2]
            }, true);

            Assert.Equal(0xAA, cpu.MemoryMap.Read(targetAddress));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }
        
    }
}