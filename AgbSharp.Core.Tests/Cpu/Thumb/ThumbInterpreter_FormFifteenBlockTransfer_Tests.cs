using AgbSharp.Core.Cpu;
using AgbSharp.Core.Memory.Ram;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormFifteenBlockTransfer_Tests
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
        }

        [Fact]
        public void Stm_StoreThreeRegistersAndPostIncrementWithWriteBack_MemoryAndRegZeroCorrect()
        {
            const uint targetAddress = InternalWramRegion.REGION_START + 0x1000;

            AgbCpu cpu = CpuUtil.CreateCpu();

            FillRegistersWithDummyData(cpu);

            cpu.CurrentRegisterSet.GetRegister(0) = targetAddress;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xC0A2 // STMIA r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress + 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.MemoryMap.ReadU32(targetAddress + (0 * 4)));
            Assert.Equal((uint)0x55555555, cpu.MemoryMap.ReadU32(targetAddress + (1 * 4)));
            Assert.Equal((uint)0x77777777, cpu.MemoryMap.ReadU32(targetAddress + (2 * 4)));
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

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xc8a2 // LDMIA r0!, {r1, r5, r7}
            }, true);

            Assert.Equal((uint)targetAddress + 0xC, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x11111111, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x55555555, cpu.CurrentRegisterSet.GetRegister(5));
            Assert.Equal((uint)0x77777777, cpu.CurrentRegisterSet.GetRegister(7));
        }
        
    }
}