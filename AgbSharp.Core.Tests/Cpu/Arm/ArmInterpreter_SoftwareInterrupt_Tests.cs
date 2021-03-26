using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Status;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_SoftwareInterrupt_Tests
    {
        [Fact]
        public void Swi_PerformSwi_ModeIsSupervisorAndCpsrTransferredToSpsrAndPcCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.RegisterValue = 0b11111000000000000000000011010000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0x000000EF // SWI
            }, true);

            Assert.Equal(CpuMode.Supervisor, cpu.CurrentStatus.Mode);
            Assert.Equal((uint)0b11111000000000000000000011010000, cpu.CurrentSavedStatus.RegisterValue);
            Assert.Equal((uint)0x00000008, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
        }

    }
}