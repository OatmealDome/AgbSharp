using AgbSharp.Core.Cpu;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormOneShiftedRegister_Tests
    {
        [Fact]
        public void ShiftedRegister_LslRegOneWithResultInRegZero_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x000001FF;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x0806 // LSL r0, r1, #24
            });

            Assert.Equal((uint)0xFF000000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x000001FF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void ShiftedRegister_LsrRegOneWithResultInRegZero_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFF800000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x080e // LSR r0, r1, #24
            });

            Assert.Equal((uint)0x000000FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFF800000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void ShiftedRegister_AsrRegOneWithResultInRegZero_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x801FF000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x4813 // ASR r0, r1, #8
            });

            Assert.Equal((uint)0xFFFC00FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x801FF000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

    }
}