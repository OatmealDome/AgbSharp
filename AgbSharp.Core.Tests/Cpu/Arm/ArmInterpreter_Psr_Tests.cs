using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Status;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_Psr_Tests
    {
        [Fact]
        public void Mrs_MoveCpsrToRegZero_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.RegisterValue = (uint)0b11111000000000000000000011011111;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE10F0000 // MRS r0, CPSR
            });

            Assert.Equal((uint)0b11111000000000000000000011011111, cpu.CurrentStatus.RegisterValue);
            Assert.Equal((uint)0b11111000000000000000000011011111, cpu.CurrentRegisterSet.GetRegister(0));
        }

        [Fact]
        public void Mrs_MoveSpsrToRegZero_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentSavedStatus = new ProgramStatus();
            cpu.CurrentSavedStatus.RegisterValue = (uint)0b11111000000000000000000011000000;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE14F0000 // MRS r0, SPSR
            });

            Assert.Equal((uint)0b11111000000000000000000011000000, cpu.CurrentSavedStatus.RegisterValue);
            Assert.Equal((uint)0b11111000000000000000000011000000, cpu.CurrentRegisterSet.GetRegister(0));
        }

        [Fact]
        public void Msr_MoveRegZeroToCpsr_CpsrCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0b11111000000000000000000011000000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE129F000 // MSR CPSR, r0
            });

            Assert.Equal((uint)0b11111000000000000000000011000000, cpu.CurrentStatus.RegisterValue);
            Assert.Equal((uint)0b11111000000000000000000011000000, cpu.CurrentRegisterSet.GetRegister(0));
        }

        [Fact]
        public void Msr_MoveRegZeroToSpsr_SpsrCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentSavedStatus = new ProgramStatus();

            cpu.CurrentRegisterSet.GetRegister(0) = 0b11111000000000000000000011000000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE169F000 // MSR SPSR, r0
            });

            Assert.Equal((uint)0b11111000000000000000000011000000, cpu.CurrentSavedStatus.RegisterValue);
            Assert.Equal((uint)0b11111000000000000000000011000000, cpu.CurrentRegisterSet.GetRegister(0));
        }

        [Fact]
        public void Msr_SetCpsrFlagsByImmediate_FlagsCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE329F20F // MSR CPSR, #0xF0000000
            });

            Assert.True(cpu.CurrentStatus.Negative);
            Assert.True(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Carry);
            Assert.True(cpu.CurrentStatus.Overflow);
        }

        [Fact]
        public void Msr_SetCpsrModeByImmediate_ModeCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE329F01F // MSR CPSR, #0x1F
            });

            Assert.Equal(CpuMode.System, cpu.CurrentStatus.Mode);
        }

    }
}