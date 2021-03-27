using AgbSharp.Core.Cpu;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormTwoAddSubtract_Tests
    {
        [Fact]
        public void Add_RegTwoAndRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00001000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x8818 // ADD r0, r1, r2
            });

            Assert.Equal((uint)0x00010000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Add_ImmediateToRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xC81D // ADD r0, r1, #7
            });

            Assert.Equal((uint)0x0000F007, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Sub_RegTwoAndRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00001000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x881A // SUB r0, r1, r2
            });

            Assert.Equal((uint)0x0000E000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Sub_ImmediateToRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xC81F // SUB r0, r1, #7
            });

            Assert.Equal((uint)0x0000F000 - 0x7, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
        }
        
    }
}