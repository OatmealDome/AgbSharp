using AgbSharp.Core.Cpu;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_Multiply_Tests
    {
        [Fact]
        public void Multiply_RegOneAndRegTwo_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFFFFFFF6;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00000014;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0000291, // MUL r0, r1, r2
            });

            Assert.Equal((uint)0xFFFFFF38, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFFFFFFF6, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00000014, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void MultiplyAccumulate_RegOneAndRegTwo_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFFFFFFF6;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00000014;
            cpu.CurrentRegisterSet.GetRegister(3) = 0x00000010;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0203291, // MLA r0, r1, r2, r3
            });

            Assert.Equal((uint)0xFFFFFF48, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFFFFFFF6, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00000014, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.Equal((uint)0x00000010, cpu.CurrentRegisterSet.GetRegister(3));
        }

        [Fact]
        public void MultiplyUnsignedLong_RegTwoAndRegThree_RegZeroAndOneCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00ABCDEF;
            cpu.CurrentRegisterSet.GetRegister(3) = 0x00FEDCBA;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0801392 // UMULL r1, r0, r2, r3
            });

            Assert.Equal((uint)0x0000AB0A, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x74EF03A6, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00ABCDEF, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.Equal((uint)0x00FEDCBA, cpu.CurrentRegisterSet.GetRegister(3));
        }

        [Fact]
        public void MultiplyAccumulateUnsignedLong_RegTwoAndRegThree_RegZeroAndOneCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0x00001000;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x00000000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00ABCDEF;
            cpu.CurrentRegisterSet.GetRegister(3) = 0x00FEDCBA;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0A01392 // UMLAL r1, r0, r2, r3
            });

            Assert.Equal((uint)0x0000BB0A, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x74EF03A6, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00ABCDEF, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.Equal((uint)0x00FEDCBA, cpu.CurrentRegisterSet.GetRegister(3));
        }

        [Fact]
        public void MultiplySignedLong_RegTwoAndRegThree_RegZeroAndOneCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(2) = 0xFFFFFFF6;
            cpu.CurrentRegisterSet.GetRegister(3) = 0x00000014;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0C01392 // SMULL r1, r0, r2, r3
            });

            Assert.Equal((uint)0xFFFFFFFF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFFFFFF38, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0xFFFFFFF6, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.Equal((uint)0x00000014, cpu.CurrentRegisterSet.GetRegister(3));
        }

        [Fact]
        public void MultiplyAccumulateSignedLong_RegTwoAndRegThree_RegZeroAndOneCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xFFFFFFFF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFFFFFFF6;
            cpu.CurrentRegisterSet.GetRegister(2) = 0xFFFFFFF6;
            cpu.CurrentRegisterSet.GetRegister(3) = 0x00000014;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0E01392 // SMLAL r1, r0, r2, r3
            });

            Assert.Equal((uint)0xFFFFFFFF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFFFFFF2E, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0xFFFFFFF6, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.Equal((uint)0x00000014, cpu.CurrentRegisterSet.GetRegister(3));
        }

    }
}