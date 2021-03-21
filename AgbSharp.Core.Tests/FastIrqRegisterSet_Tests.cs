using System.Collections.Generic;
using AgbSharp.Core.Cpu.Register;
using Xunit;

namespace AgbSharp.Core.Tests
{
    public class FastIrqRegisterSet_Tests
    {
        [Theory]
        [MemberData(nameof(SameRegisters))]
        public void GetSet_Register_Equal(int regNum)
        {
            IRegisterSet baseSet = new BaseRegisterSet();
            IRegisterSet overrideSet = new FastIrqRegisterSet(baseSet);

            baseSet.GetRegister(regNum) = 0xFFFFFFFF;

            Assert.Equal(0xFFFFFFFF, overrideSet.GetRegister(regNum));
        }

        [Theory]
        [MemberData(nameof(OverriddenRegisters))]
        public void GetSet_SetOverriddenRegisterInOverride_Equal(int regNum)
        {
            IRegisterSet baseSet = new BaseRegisterSet();
            IRegisterSet overrideSet = new FastIrqRegisterSet(baseSet);

            overrideSet.GetRegister(regNum) = 0xFFFFFFFF;

            Assert.Equal(0xFFFFFFFF, overrideSet.GetRegister(regNum));
        }

        [Theory]
        [MemberData(nameof(OverriddenRegisters))]
        public void GetSet_SetOverriddenRegisterInBase_NotEqual(int regNum)
        {
            IRegisterSet baseSet = new BaseRegisterSet();
            IRegisterSet overrideSet = new FastIrqRegisterSet(baseSet);

            baseSet.GetRegister(regNum) = 0xFFFFFFFF;

            Assert.NotEqual(0xFFFFFFFF, overrideSet.GetRegister(regNum));
        }

        [Theory]
        [MemberData(nameof(OverriddenRegisters))]
        public void GetSet_SetDifferentValuesForBaseAndOverride_NotEqual(int regNum)
        {
            IRegisterSet baseSet = new BaseRegisterSet();
            IRegisterSet overrideSet = new FastIrqRegisterSet(baseSet);

            baseSet.GetRegister(regNum) = 0xFFFFFFFF;
            overrideSet.GetRegister(regNum) = 0xFFFFFFFE;

            Assert.NotEqual(0xFFFFFFFF, overrideSet.GetRegister(regNum));
        }

        public static IEnumerable<object[]> SameRegisters => new List<object[]>
        {
            new object[] { 1 },
            new object[] { 2 },
            new object[] { 3 },
            new object[] { 4 },
            new object[] { 5 },
            new object[] { 6 },
            new object[] { 7 },
            new object[] { 15 }
        };

        public static IEnumerable<object[]> OverriddenRegisters => new List<object[]>
        {
            new object[] { 8 },
            new object[] { 9 },
            new object[] { 10 },
            new object[] { 11 },
            new object[] { 12 },
            new object[] { 13 },
            new object[] { 14 }
        };

    }
}
