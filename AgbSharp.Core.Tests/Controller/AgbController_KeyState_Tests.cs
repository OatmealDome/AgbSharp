using System.Collections.Generic;
using AgbSharp.Core.Controller;
using AgbSharp.Core.Memory;
using AgbSharp.Core.Util;
using Xunit;

namespace AgbSharp.Core.Tests.Controller
{
    public class AgbController_KeyState_Tests
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