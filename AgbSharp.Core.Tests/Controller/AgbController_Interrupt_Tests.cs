using AgbSharp.Core.Controller;
using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Status;
using AgbSharp.Core.Memory;
using Xunit;

namespace AgbSharp.Core.Tests.Controller
{
    public class AgbController_Interrupt_Tests
    {
        [Fact]
        public void UpdateKeyState_ChangeBStateToPressedWithLogicalOrInterrupts_CpuInterrupted()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();
            AgbCpu cpu = new AgbCpu(memoryMap);
            AgbController controller = new AgbController(memoryMap, cpu);

            memoryMap.WriteU32(0x4000208, 1); // IME = 1
            memoryMap.WriteU16(0x4000200, 0x1000); // IE = Key
            memoryMap.WriteU16(0x4000132, 0x4003); // Key interrupts enabled, logical OR, A or B

            memoryMap.FlushMmio();

            controller.UpdateKeyState(ControllerKey.B, true);

            Assert.Equal(CpuMode.Irq, cpu.CurrentStatus.Mode);
        }

        [Fact]
        public void UpdateKeyState_ChangeAStateToPressedWithLogicalOrInterrupts_CpuInterrupted()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();
            AgbCpu cpu = new AgbCpu(memoryMap);
            AgbController controller = new AgbController(memoryMap, cpu);

            memoryMap.WriteU32(0x4000208, 1); // IME = 1
            memoryMap.WriteU16(0x4000200, 0x1000); // IE = Key
            memoryMap.WriteU16(0x4000132, 0x4003); // Key interrupts enabled, logical OR, A or B

            memoryMap.FlushMmio();

            controller.UpdateKeyState(ControllerKey.A, true);

            Assert.Equal(CpuMode.Irq, cpu.CurrentStatus.Mode);
        }

        [Fact]
        public void UpdateKeyState_ChangeUpStateToPressedWithLogicalOrInterrupts_CpuNotInterrupted()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();
            AgbCpu cpu = new AgbCpu(memoryMap);
            AgbController controller = new AgbController(memoryMap, cpu);

            memoryMap.WriteU32(0x4000208, 1); // IME = 1
            memoryMap.WriteU16(0x4000200, 0x1000); // IE = Key
            memoryMap.WriteU16(0x4000132, 0x4003); // Key interrupts enabled, logical OR, A or B

            memoryMap.FlushMmio();

            controller.UpdateKeyState(ControllerKey.Up, true);

            Assert.Equal(CpuMode.User, cpu.CurrentStatus.Mode);
        }

        [Fact]
        public void UpdateKeyState_ChangeStateToPressedWithLogicalAndInterrupts_CpuInterrupted()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();
            AgbCpu cpu = new AgbCpu(memoryMap);
            AgbController controller = new AgbController(memoryMap, cpu);

            memoryMap.WriteU32(0x4000208, 1); // IME = 1
            memoryMap.WriteU16(0x4000200, 0x1000); // IE = Key
            memoryMap.WriteU16(0x4000132, 0xC003); // Key interrupts enabled, logical AND, A and B

            memoryMap.FlushMmio();

            controller.UpdateKeyState(ControllerKey.A, true);
            controller.UpdateKeyState(ControllerKey.B, true);

            Assert.Equal(CpuMode.Irq, cpu.CurrentStatus.Mode);
        }

        [Fact]
        public void UpdateKeyState_OnlyAPressedWithLogicalAndInterrupts_CpuNotInterrupted()
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();
            AgbCpu cpu = new AgbCpu(memoryMap);
            AgbController controller = new AgbController(memoryMap, cpu);

            memoryMap.WriteU32(0x4000208, 1); // IME = 1
            memoryMap.WriteU16(0x4000200, 0x1000); // IE = Key
            memoryMap.WriteU16(0x4000132, 0xC003); // Key interrupts enabled, logical AND, A and B

            memoryMap.FlushMmio();

            controller.UpdateKeyState(ControllerKey.A, true);

            Assert.Equal(CpuMode.User, cpu.CurrentStatus.Mode);
        }

    }
}