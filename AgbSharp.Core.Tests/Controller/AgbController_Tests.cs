using System.Collections.Generic;
using AgbSharp.Core.Controller;
using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Status;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Util;
using Xunit;

namespace AgbSharp.Core.Tests.Controller
{
    public class AgbController_Tests
    {
        [Theory]
        [MemberData(nameof(Keys))]
        public void UpdateKeyState_ChangeStateToPressedAndCheckBitfield_BitSet(ControllerKey key)
        {
            AgbMemoryMap memoryMap = new AgbMemoryMap();
            AgbController controller = new AgbController(memoryMap, null);

            controller.UpdateKeyState(key, true);

            ushort mmioValue = memoryMap.ReadU16(0x4000130);

            Assert.True(BitUtil.IsBitSet(mmioValue, (int)key));
        }

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
        public void UpdateKeyState_ChangeStateToPressedWithLogicalAndInterrupts_BitSet()
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

        public static IEnumerable<object[]> Keys => new List<object[]>
        {
            new object[] { ControllerKey.A },
            new object[] { ControllerKey.B },
            new object[] { ControllerKey.Select },
            new object[] { ControllerKey.Start },
            new object[] { ControllerKey.Right },
            new object[] { ControllerKey.Left },
            new object[] { ControllerKey.Up },
            new object[] { ControllerKey.Down },
            new object[] { ControllerKey.R },
            new object[] { ControllerKey.L }
        };

    }
}