using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_LoadStore_Tests
    {
        [Fact]
        public void Load_UsingAddressInRegOne_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x000091E5 // LDR r0, [r1]
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Load_UsingAddressInRegOneAndOffsetInRegTwo_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x020091E7 // LDR r0, [r1, r2]
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Load_UsingAddressInRegOneAndNegativeOffsetInRegTwo_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress + 0x1000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x020011E7 // LDR r0, [r1, -r2]
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress + 0x1000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Load_UsingAddressInRegOneAndShiftedOffsetInRegTwo_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x10;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x020491E7 // LDR r0, [r1, r2, LSL#8]
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x10, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Load_UsingAddressInRegOneAndOffsetInRegTwoWithWriteBack_LoadSuccessAndRegOneCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x0200B1E7 // LDR r0, [r1, r2]!
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Load_UsingAddressInRegOneWithPostIndexedWriteBack_LoadSuccessAndRegOneCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START + 0x1000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x100091E4 // LDR r0, [r1], #0x10
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress + 0x10, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Load_UsingHalfWordAlignedAddressInRegOne_LoadHalfWordSuccess()
        {
            const uint baseAddress = InternalWramRegion.REGION_START + 0x1000;
            const uint targetAddress = baseAddress + 0x2;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFEBABE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x000091E5 // LDR r0, [r1]
            }, true);

            Assert.Equal((uint)0x0000BABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void LoadByte_UsingAddressInRegOne_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.Write(targetAddress, 0xAA);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x0000D1E5 // LDRB r0, [r1]
            }, true);

            Assert.Equal((uint)0x000000AA, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Store_UsingAddressInRegOne_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x000081E5 // STR r0, [r1]
            }, true);

            Assert.Equal(0xCAFEBABE, cpu.MemoryMap.ReadU32(targetAddress));
            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void StoreByte_UsingAddressInRegOne_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0x000000AA;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x0000C1E5 // STRB r0, [r1]
            }, true);

            Assert.Equal(0xAA, cpu.MemoryMap.Read(targetAddress));
            Assert.Equal((uint)0x000000AA, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }

    }
}