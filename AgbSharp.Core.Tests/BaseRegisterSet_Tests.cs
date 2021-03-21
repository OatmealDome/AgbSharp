using System.Collections.Generic;
using AgbSharp.Core.Cpu.Register;
using Xunit;

namespace AgbSharp.Core.Tests
{
    public class BaseRegisterSet_Tests
    {
        [Theory]
        [MemberData(nameof(Registers))]
        public void GetSet_Register_Equal(int regNum)
        {
            IRegisterSet registerSet = new BaseRegisterSet();

            registerSet.GetRegister(regNum) = 0xFFFFFFFF;

            Assert.Equal(0xFFFFFFFF, registerSet.GetRegister(regNum));
        }

        public static IEnumerable<object[]> Registers => new List<object[]>
        {
            new object[] { 1 },
            new object[] { 2 },
            new object[] { 3 },
            new object[] { 4 },
            new object[] { 5 },
            new object[] { 6 },
            new object[] { 7 },
            new object[] { 8 },
            new object[] { 9 },
            new object[] { 10 },
            new object[] { 11 },
            new object[] { 12 },
            new object[] { 13 },
            new object[] { 14 },
            new object[] { 15 }
        };

    }
}
