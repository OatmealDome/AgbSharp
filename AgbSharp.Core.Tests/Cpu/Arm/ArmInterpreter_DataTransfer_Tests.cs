using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Status;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_DataTransfer_Tests
    {
        private static void FillRegistersWithDummyData(AgbCpu cpu)
        {
            cpu.CurrentRegisterSet.GetRegister(0) = 0x01020304;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x11111111;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x22222222;
            cpu.CurrentRegisterSet.GetRegister(3) = 0x33333333;
            cpu.CurrentRegisterSet.GetRegister(4) = 0x44444444;
            cpu.CurrentRegisterSet.GetRegister(5) = 0x55555555;
            cpu.CurrentRegisterSet.GetRegister(6) = 0x66666666;
            cpu.CurrentRegisterSet.GetRegister(7) = 0x77777777;
            cpu.CurrentRegisterSet.GetRegister(8) = 0x88888888;
            cpu.CurrentRegisterSet.GetRegister(9) = 0x99999999;
            cpu.CurrentRegisterSet.GetRegister(10) = 0xAAAAAAAA;
            cpu.CurrentRegisterSet.GetRegister(11) = 0xBBBBBBBB;
            cpu.CurrentRegisterSet.GetRegister(12) = 0xCCCCCCCC;
            cpu.CurrentRegisterSet.GetRegister(13) = 0xDDDDDDDD;
            cpu.CurrentRegisterSet.GetRegister(14) = 0xEEEEEEEE;
            cpu.CurrentRegisterSet.GetRegister(15) = 0xFFFFFFFF;
            
            // All non-User registers will be filled with nothing
            cpu.CurrentStatus.Mode = CpuMode.System;
        }

        [Fact]
        public void Stm_StoreAllRegistersExceptPcAndPostIncrement_MemoryCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            FillRegistersWithDummyData(cpu);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xFEFF80E8 // STMIA r0, {r1-r14}
            }, true);

            Assert.Equal((uint)0x11111111, cpu.MemoryMap.ReadU32(targetAddress + (0 * 4)));
            Assert.Equal((uint)0x22222222, cpu.MemoryMap.ReadU32(targetAddress + (1 * 4)));
            Assert.Equal((uint)0x33333333, cpu.MemoryMap.ReadU32(targetAddress + (2 * 4)));
            Assert.Equal((uint)0x44444444, cpu.MemoryMap.ReadU32(targetAddress + (3 * 4)));
            Assert.Equal((uint)0x55555555, cpu.MemoryMap.ReadU32(targetAddress + (4 * 4)));
            Assert.Equal((uint)0x66666666, cpu.MemoryMap.ReadU32(targetAddress + (5 * 4)));
            Assert.Equal((uint)0x77777777, cpu.MemoryMap.ReadU32(targetAddress + (6 * 4)));
            Assert.Equal((uint)0x88888888, cpu.MemoryMap.ReadU32(targetAddress + (7 * 4)));
            Assert.Equal((uint)0x99999999, cpu.MemoryMap.ReadU32(targetAddress + (8 * 4)));
            Assert.Equal((uint)0xAAAAAAAA, cpu.MemoryMap.ReadU32(targetAddress + (9 * 4)));
            Assert.Equal((uint)0xBBBBBBBB, cpu.MemoryMap.ReadU32(targetAddress + (10 * 4)));
            Assert.Equal((uint)0xCCCCCCCC, cpu.MemoryMap.ReadU32(targetAddress + (11 * 4)));
            Assert.Equal((uint)0xDDDDDDDD, cpu.MemoryMap.ReadU32(targetAddress + (12 * 4)));
            Assert.Equal((uint)0xEEEEEEEE, cpu.MemoryMap.ReadU32(targetAddress + (13 * 4)));
            Assert.Equal(InternalWramRegion.REGION_START + 4, cpu.MemoryMap.ReadU32(targetAddress + (14 * 4)));
        }

        [Fact]
        public void Stm_StoreThreeRegistersAndPostIncrementWithWriteBack_MemoryAndRegZeroCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            FillRegistersWithDummyData(cpu);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xA200A0E8 // STMIA r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress + 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.MemoryMap.ReadU32(targetAddress + (0 * 4)));
            Assert.Equal((uint)0x55555555, cpu.MemoryMap.ReadU32(targetAddress + (1 * 4)));
            Assert.Equal((uint)0x77777777, cpu.MemoryMap.ReadU32(targetAddress + (2 * 4)));
        }

        [Fact]
        public void Stm_StoreThreeRegistersAndPreIncrementWithWriteBack_MemoryAndRegZeroCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            FillRegistersWithDummyData(cpu);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xA200A0E9 // STMIB r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress + 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.MemoryMap.ReadU32(targetAddress + (1 * 4)));
            Assert.Equal((uint)0x55555555, cpu.MemoryMap.ReadU32(targetAddress + (2 * 4)));
            Assert.Equal((uint)0x77777777, cpu.MemoryMap.ReadU32(targetAddress + (3 * 4)));
        }

        [Fact]
        public void Stm_StoreThreeRegistersAndPostDecrementWithWriteBack_MemoryAndRegZeroCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            FillRegistersWithDummyData(cpu);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xA20020E8 // STMDA r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress - 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.MemoryMap.ReadU32(targetAddress - (0 * 4)));
            Assert.Equal((uint)0x55555555, cpu.MemoryMap.ReadU32(targetAddress - (1 * 4)));
            Assert.Equal((uint)0x77777777, cpu.MemoryMap.ReadU32(targetAddress - (2 * 4)));
        }

        [Fact]
        public void Stm_StoreThreeRegistersAndPreDecrementWithWriteBack_MemoryAndRegZeroCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            FillRegistersWithDummyData(cpu);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xA20020E9 // STMDB r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress - 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.MemoryMap.ReadU32(targetAddress - (3 * 4)));
            Assert.Equal((uint)0x55555555, cpu.MemoryMap.ReadU32(targetAddress - (2 * 4)));
            Assert.Equal((uint)0x77777777, cpu.MemoryMap.ReadU32(targetAddress - (1 * 4)));
        }

        [Fact]
        public void Ldm_LoadAllRegistersAndPostIncrement_RegistersCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress + (0 * 4), 0x11111111);
            cpu.MemoryMap.WriteU32(targetAddress + (1 * 4), 0x22222222);
            cpu.MemoryMap.WriteU32(targetAddress + (2 * 4), 0x33333333);
            cpu.MemoryMap.WriteU32(targetAddress + (3 * 4), 0x44444444);
            cpu.MemoryMap.WriteU32(targetAddress + (4 * 4), 0x55555555);
            cpu.MemoryMap.WriteU32(targetAddress + (5 * 4), 0x66666666);
            cpu.MemoryMap.WriteU32(targetAddress + (6 * 4), 0x77777777);
            cpu.MemoryMap.WriteU32(targetAddress + (7 * 4), 0x88888888);
            cpu.MemoryMap.WriteU32(targetAddress + (8 * 4), 0x99999999);
            cpu.MemoryMap.WriteU32(targetAddress + (9 * 4), 0xAAAAAAAA);
            cpu.MemoryMap.WriteU32(targetAddress + (10 * 4), 0xBBBBBBBB);
            cpu.MemoryMap.WriteU32(targetAddress + (11 * 4), 0xCCCCCCCC);
            cpu.MemoryMap.WriteU32(targetAddress + (12 * 4), 0xDDDDDDDD);
            cpu.MemoryMap.WriteU32(targetAddress + (13 * 4), 0xEEEEEEEE);
            cpu.MemoryMap.WriteU32(targetAddress + (14 * 4), 0xFFFFFFFF);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xFEFF90E8 // LDMIA r0, {r1-r15}
            }, true);

            Assert.Equal((uint)0x11111111, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x22222222, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.Equal((uint)0x33333333, cpu.CurrentRegisterSet.GetRegister(3));
            Assert.Equal((uint)0x44444444, cpu.CurrentRegisterSet.GetRegister(4));
            Assert.Equal((uint)0x55555555, cpu.CurrentRegisterSet.GetRegister(5));
            Assert.Equal((uint)0x66666666, cpu.CurrentRegisterSet.GetRegister(6));
            Assert.Equal((uint)0x77777777, cpu.CurrentRegisterSet.GetRegister(7));
            Assert.Equal((uint)0x88888888, cpu.CurrentRegisterSet.GetRegister(8));
            Assert.Equal((uint)0x99999999, cpu.CurrentRegisterSet.GetRegister(9));
            Assert.Equal((uint)0xAAAAAAAA, cpu.CurrentRegisterSet.GetRegister(10));
            Assert.Equal((uint)0xBBBBBBBB, cpu.CurrentRegisterSet.GetRegister(11));
            Assert.Equal((uint)0xCCCCCCCC, cpu.CurrentRegisterSet.GetRegister(12));
            Assert.Equal((uint)0xDDDDDDDD, cpu.CurrentRegisterSet.GetRegister(13));
            Assert.Equal((uint)0xEEEEEEEE, cpu.CurrentRegisterSet.GetRegister(14));
            Assert.Equal((uint)0xFFFFFFFF, cpu.CurrentRegisterSet.GetRegister(15));
        }

        [Fact]
        public void Ldm_LoadThreeRegistersAndPostIncrementWithWriteBack_RegistersCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress + (0 * 4), 0x11111111);
            cpu.MemoryMap.WriteU32(targetAddress + (1 * 4), 0x55555555);
            cpu.MemoryMap.WriteU32(targetAddress + (2 * 4), 0x77777777);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xA200B0E8 // LDMIA r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress + 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x55555555, cpu.CurrentRegisterSet.GetRegister(5));
            Assert.Equal((uint)0x77777777, cpu.CurrentRegisterSet.GetRegister(7));
        }

        [Fact]
        public void Ldm_LoadThreeRegistersAndPreIncrementWithWriteBack_RegistersCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress + (1 * 4), 0x11111111);
            cpu.MemoryMap.WriteU32(targetAddress + (2 * 4), 0x55555555);
            cpu.MemoryMap.WriteU32(targetAddress + (3 * 4), 0x77777777);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xA200B0E9 // LDMIB r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress + 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x55555555, cpu.CurrentRegisterSet.GetRegister(5));
            Assert.Equal((uint)0x77777777, cpu.CurrentRegisterSet.GetRegister(7));
        }

        [Fact]
        public void Ldm_LoadThreeRegistersAndPostDecrementWithWriteBack_RegistersCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress - (0 * 4), 0x11111111);
            cpu.MemoryMap.WriteU32(targetAddress - (1 * 4), 0x55555555);
            cpu.MemoryMap.WriteU32(targetAddress - (2 * 4), 0x77777777);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xA20030E8 // LDMDA r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress - 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x55555555, cpu.CurrentRegisterSet.GetRegister(5));
            Assert.Equal((uint)0x77777777, cpu.CurrentRegisterSet.GetRegister(7));
        }

        [Fact]
        public void Ldm_LoadThreeRegistersAndPreDecrementWithWriteBack_RegistersCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.MemoryMap.WriteU32(targetAddress - (3 * 4), 0x11111111);
            cpu.MemoryMap.WriteU32(targetAddress - (2 * 4), 0x55555555);
            cpu.MemoryMap.WriteU32(targetAddress - (1 * 4), 0x77777777);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xA20030E9 // LDMDB r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress - 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x55555555, cpu.CurrentRegisterSet.GetRegister(5));
            Assert.Equal((uint)0x77777777, cpu.CurrentRegisterSet.GetRegister(7));
        }

    }
}