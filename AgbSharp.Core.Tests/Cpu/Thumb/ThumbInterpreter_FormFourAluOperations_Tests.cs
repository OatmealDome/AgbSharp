using AgbSharp.Core.Cpu;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Thumb
{
    public class ThumbInterpreter_FormFourAluOperations_Tests
    {
        #region AND

        [Fact]
        public void And_AndRegOneAndRegTwo_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFFFF0000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x0840 // AND r0, r1
            });

            Assert.Equal(0xCAFE0000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(0xFFFF0000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Negative);
        }

        [Fact]
        public void Tst_AndRegOneAndRegTwo_FlagsCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFFFF0000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x0842 // TST r0, r1
            });

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(0xFFFF0000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Negative);
        }

        #endregion

        #region EOR

        [Fact]
        public void Eor_RegOneWithRegTwo_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x4840 // EOR r0, r1
            });

            Assert.Equal((uint)0x14530451, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(1));
        }

        #endregion
    
        #region Shifts

        [Fact]
        public void Lsl_ShiftRegZeroLeftByRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0x000001FF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFEFEFE18;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x8840 // LSL r0, r1
            });

            Assert.Equal((uint)0xFF000000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFEFEFE18, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Lsr_ShiftRegZeroRightByRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xFF800000;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFEFEFE18;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xc840 // LSR r0, r1
            });

            Assert.Equal((uint)0x000000FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFEFEFE18, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Asr_ArithmaticShiftRegZeroRightByRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0x801FF000;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFEFEFE0D;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x0841 // ASR r0, r1
            });

            Assert.Equal((uint)0xFFFC00FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFEFEFE0D, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithRor_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0x0000FFFF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFEFEFE08;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xc841 // ROR r0, r1
            });

            Assert.Equal((uint)0xFF0000FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFEFEFE08, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        #endregion

        #region ADD

        [Fact]
        public void AddCarry_RegOneAndRegZeroWithCarry_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = true;

            cpu.CurrentRegisterSet.GetRegister(0) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x00001000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x4841 // ADC r0, r1
            });

            Assert.Equal((uint)0x00010001, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Cmn_RegTwoFromRegOne_FlagsSet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = false;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFFFFFFFF;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00000001;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xd142 // CMN r1, r2
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFFFFFFFF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00000001, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.False(cpu.CurrentStatus.Negative);
            Assert.True(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Carry);
            Assert.False(cpu.CurrentStatus.Overflow);
        }

        #endregion

        #region SUB

        [Fact]
        public void SubtractCarry_RegOneFromRegZeroWithCarry_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = false;

            cpu.CurrentRegisterSet.GetRegister(0) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x00001000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x8841 // SBC r0, r1
            });

            Assert.Equal((uint)0x0000DFFF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Negate_SubtractZeroFromRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x00001000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x4842 // NEG r0, r1
            });

            Assert.Equal((uint)0xFFFFF000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Compare_RegTwoFromRegOne_FlagsSet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00001000;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x9142 // CMP r1, r2
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.False(cpu.CurrentStatus.Negative);
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Carry);
            Assert.False(cpu.CurrentStatus.Overflow);
        }

        #endregion
    
        #region ORR

        [Fact]
        public void Orr_OrRegZeroAndRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xCAFE0000;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000BABE;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x0843 // AND r0, r1
            });

            Assert.Equal((uint)0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000BABE, cpu.CurrentRegisterSet.GetRegister(1));
        }
        
        #endregion

        #region MUL

        [Fact]
        public void Multiply_RegOneAndRegTwo_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xFFFFFFF6;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x00000014;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x4843, // MUL r0, r1
            });

            Assert.Equal((uint)0xFFFFFF38, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x00000014, cpu.CurrentRegisterSet.GetRegister(1));
        }

        #endregion

        #region BIC

        [Fact]
        public void BitClear_BicWithRegZeroAndRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0x000000FF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFFFFFF00;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0x8843 // BIC r0, r1
            });

            Assert.Equal((uint)0x000000FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFFFFFF00, cpu.CurrentRegisterSet.GetRegister(1));
        }

        #endregion

        #region MVN

        [Fact]
        public void MoveNegative_MvnWithRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x000000FF;

            CpuUtil.RunCpu(cpu, new ushort[]
            {
                0xc843 // MVN r0, r1
            });

            Assert.Equal((uint)0xFFFFFF00, cpu.CurrentRegisterSet.GetRegister(0));
        }

        #endregion

    }
}