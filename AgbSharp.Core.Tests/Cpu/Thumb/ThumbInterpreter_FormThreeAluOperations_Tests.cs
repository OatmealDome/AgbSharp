using AgbSharp.Core.Cpu;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormThreeAluOperations_Tests
    {
        [Fact]
        public void Move_MoveImmediateToRegZero_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xff20 // MOV r0, #0xff
            });

            Assert.Equal((uint)0x000000FF, cpu.CurrentRegisterSet.GetRegister(0));
        }

        [Fact]
        public void Compare_SubtractImmediateFromRegZero_RegZeroUntouchedAndFlagsCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xff28 // CMP r0, #0xff
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.True(cpu.CurrentStatus.Negative);
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Carry);
            Assert.False(cpu.CurrentStatus.Overflow);
        }

        [Fact]
        public void Add_AddImmediateToRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0x0000F000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xff30 // ADD r0, #0xff
            });

            Assert.Equal((uint)0x0000F0FF, cpu.CurrentRegisterSet.GetRegister(0));
        }

        [Fact]
        public void Subtract_SubtractImmediateFromRegZero_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0x0000F000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xff38 // SUB r0, #0xff
            });

            Assert.Equal((uint)0x0000EF01, cpu.CurrentRegisterSet.GetRegister(0));
        }

    }
}