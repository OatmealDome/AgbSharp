using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_LoadStoreSignedHalf_Tests
    {
        [Fact]
        public void LoadHalf_UsingAddressInRegOne_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU16(targetAddress, 0xCAFE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xB000D1E1 // LDRH r0, [r1]
            }, true);

            Assert.Equal((uint)0x0000CAFE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }
        
        [Fact]
        public void LoadHalf_UsingUnalignedAddressInRegOne_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU16(targetAddress, 0xCAFE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress + 1;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xB000D1E1 // LDRH r0, [r1]
            }, true);

            Assert.Equal(0xFE0000CA, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress + 1, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void LoadHalf_UsingAddressInRegOneAndOffsetInRegTwo_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU16(targetAddress, 0xCAFE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xB20091E1 // LDRH r0, [r1, r2]
            }, true);

            Assert.Equal((uint)0x0000CAFE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(InternalWramRegion.REGION_START, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void LoadHalf_UsingAddressInRegOneAndNegativeOffsetInRegTwo_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU16(targetAddress, 0xCAFE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress + 0x1000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xB20011E1 // LDRH r0, [r1, -r2]
            }, true);

            Assert.Equal((uint)0x0000CAFE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress + 0x1000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void LoadHalf_UsingAddressInRegOneAndOffsetInRegTwoWithWriteBack_LoadSuccessAndRegOneCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress, 0xCAFE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = InternalWramRegion.REGION_START;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x1000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xB200B1E1 // LDRH r0, [r1, r2]!
            }, true);

            Assert.Equal((uint)0x0000CAFE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x1000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void LoadHalf_UsingAddressInRegOneWithPostIndexedWriteBack_LoadSuccessAndRegOneCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU16(targetAddress, 0xCAFE);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xB001D1E0 // LDRH r0, [r1], #0x10
            }, true);

            Assert.Equal((uint)0x0000CAFE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress + 0x10, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void LoadSignedByte_UsingAddressInRegOneWithPositiveValue_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.Write(targetAddress, 0x7F);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xD000D1E1 // LDRSB r0, [r1]
            }, true);

            Assert.Equal((uint)0x0000007F, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void LoadSignedByte_UsingAddressInRegOneWithNegativeValue_LoadSuccessAndSignExtended()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.Write(targetAddress, 0x80);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xD000D1E1 // LDRSB r0, [r1]
            }, true);

            Assert.Equal((uint)0xFFFFFF80, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void LoadSignedHalf_UsingAddressInRegOneWithPositiveValue_LoadSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU16(targetAddress, 0x7F00);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xF000D1E1 // LDRSH r0, [r1]
            }, true);

            Assert.Equal((uint)0x00007F00, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void LoadSignedHalf_UsingAddressInRegOneWithNegativeValue_LoadSuccessAndSignExtended()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU16(targetAddress, 0x8000);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xF000D1E1 // LDRSH r0, [r1]
            }, true);

            Assert.Equal((uint)0xFFFF8000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }
        
        [Fact]
        public void LoadSignedHalf_UsingUnalignedAddressInRegOne_LoadSuccessAndSignExtended()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU16(targetAddress, 0xFF00);

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress + 1;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xF000D1E1 // LDRSH r0, [r1]
            }, true);

            Assert.Equal(0xFFFFFFFF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(targetAddress + 1, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void StoreHalf_UsingAddressInRegOne_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xFFFFCAFE;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xB000C1E1 // STRH r0, [r1]
            }, true);

            Assert.Equal((uint)0x0000CAFE, cpu.MemoryMap.ReadU16(targetAddress));
            Assert.Equal(targetAddress, cpu.CurrentRegisterSet.GetRegister(1));
        }
        
        [Fact]
        public void StoreHalf_UsingUnalignedAddressInRegOne_StoreSuccess()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xFFFFCAFE;
            cpu.CurrentRegisterSet.GetRegister(1) = targetAddress + 1;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xB000C1E1 // STRH r0, [r1]
            }, true);

            Assert.Equal((uint)0x0000CAFE, cpu.MemoryMap.ReadU16(targetAddress));
            Assert.Equal(targetAddress + 1, cpu.CurrentRegisterSet.GetRegister(1));
        }
    }
}