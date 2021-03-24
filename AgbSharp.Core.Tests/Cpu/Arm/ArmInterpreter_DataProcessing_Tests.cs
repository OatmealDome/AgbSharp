using AgbSharp.Core.Cpu;
using AgbSharp.Core.Cpu.Status;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu.Arm
{
    public class ArmInterpreter_DataProcessing_Tests
    {
        #region AND

        [Fact]
        public void And_AndRegOneAndRegTwo_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(2) = 0xFFFF0000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0010002 // AND r0, r1, r2
            });

            Assert.Equal(0xCAFE0000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal(0xFFFF0000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void And_AndRegOneAndImmediate_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFEBABE;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE2010CFF // AND r1, #0xFF00
            });

            Assert.Equal((uint)0x0000BA00, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Tst_AndRegOneAndRegTwo_FlagsCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(2) = 0xFFFF0000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1110002 // TST r1, r2
            });

            Assert.Equal(0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal(0xFFFF0000, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Negative);
        }

        #endregion

        #region TST

        [Fact]
        public void Tst_AndRegOneAndImmediate_FlagsCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFEBABE;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE3110CFF // TST r1, #0xFF00
            });

            Assert.Equal(0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.False(cpu.CurrentStatus.Negative);
        }

        [Fact]
        public void Tst_AndRegOneAndZeroImmediate_FlagsCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFEBABE;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE3110000 // TST r1, #0x0
            });

            Assert.Equal(0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Zero);
            Assert.False(cpu.CurrentStatus.Negative);
        }

        #endregion

        #region EOR

        [Fact]
        public void Eor_RegOneWithRegTwo_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xFEEDFACE;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFEBABE;
            cpu.CurrentRegisterSet.GetRegister(2) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0210002 // EOR r0, r1, r2
            });

            Assert.Equal((uint)0x14530451, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Teq_RegOneWithRegTwo_FlagsCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xFEEDFACE;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFFFFFFFF;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x7FFFFFFF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1310002 // TEQ r1, r2
            });

            Assert.Equal((uint)0xFEEDFACE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFFFFFFFF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x7FFFFFFF, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.True(cpu.CurrentStatus.Negative);
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.False(cpu.CurrentStatus.Carry);
            Assert.False(cpu.CurrentStatus.Overflow);
        }

        #endregion

        #region SUB

        [Fact]
        public void Subtract_RegTwoFromRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00001000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0410002 // SUB r0, r1, r2
            });

            Assert.Equal((uint)0x0000E000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void SubtractReverse_RegTwoFromRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x00001000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x0000F000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0610002 // RSB r0, r1, r2
            });

            Assert.Equal((uint)0x0000E000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void SubtractCarry_RegTwoFromRegOneWithCarry_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = false;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00001000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0C10002 // SBC r0, r1, r2
            });

            Assert.Equal((uint)0x0000DFFF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(2));
        }


        [Fact]
        public void SubtractReverseCarry_RegTwoFromRegOneWithCarry_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = false;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x00001000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x0000F000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0E10002 // RSC r0, r1, r2
            });

            Assert.Equal((uint)0x0000DFFF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Compare_RegTwoFromRegOne_FlagsSet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = false;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00001000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1510002 // CMP r1, r2
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.False(cpu.CurrentStatus.Negative);
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Carry);
            Assert.False(cpu.CurrentStatus.Overflow);
        }

        [Fact]
        public void Compare_RegTwoFromRegOneWithOverflow_FlagsSet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = false;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x80000000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00000001;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1510002 // CMP r1, r2
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x80000000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00000001, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.False(cpu.CurrentStatus.Negative);
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Carry);
            Assert.True(cpu.CurrentStatus.Overflow);
        }

        #endregion

        #region ADD

        [Fact]
        public void Add_RegTwoAndRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00001000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0810002 // ADD r0, r1, r2
            });

            Assert.Equal((uint)0x00010000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void AddCarry_RegTwoAndRegOneWithCarry_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = true;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000F000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00001000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0A10002 // ADC r0, r1, r2
            });

            Assert.Equal((uint)0x00010001, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000F000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00001000, cpu.CurrentRegisterSet.GetRegister(2));
        }

        [Fact]
        public void Cmn_RegTwoFromRegOne_FlagsSet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = false;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFFFFFFFF;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00000001;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1710002 // CMN r1, r2
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFFFFFFFF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00000001, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.False(cpu.CurrentStatus.Negative);
            Assert.True(cpu.CurrentStatus.Zero);
            Assert.True(cpu.CurrentStatus.Carry);
            Assert.False(cpu.CurrentStatus.Overflow);
        }

        [Fact]
        public void Cmn_RegTwoFromRegOneWithOverflow_FlagsSet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentStatus.Carry = false;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x7FFFFFFF;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x00000001;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1710002 // CMN r1, r2
            });

            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x7FFFFFFF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x00000001, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.True(cpu.CurrentStatus.Negative);
            Assert.False(cpu.CurrentStatus.Zero);
            Assert.False(cpu.CurrentStatus.Carry);
            Assert.True(cpu.CurrentStatus.Overflow);
        }

        #endregion

        #region ORR
        
        [Fact]
        public void Orr_OrRegOneAndRegTwo_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFE0000;
            cpu.CurrentRegisterSet.GetRegister(2) = 0x0000BABE;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE0810002 // AND r0, r1, r2
            });

            Assert.Equal((uint)0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xCAFE0000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0x0000BABE, cpu.CurrentRegisterSet.GetRegister(2));
        }

        #endregion

        #region MOV

        [Fact]
        public void Move_MoveRegOneToRegZero_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xCAFEBABE;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A00001 // MOV r0, r1
            });

            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal(0xCAFEBABE, cpu.CurrentRegisterSet.GetRegister(1));
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithLsl_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x000001FF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A00C01 // MOV r0, r1, LSL#24
            });

            Assert.Equal((uint)0xFF000000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x000001FF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithLsr_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFF800000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A00C21 // MOV r0, r1, LSR#24
            });

            Assert.Equal((uint)0x000000FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFF800000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithAsr_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x801FF000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A006C1 // MOV r0, r1, ASR#13
            });

            Assert.Equal((uint)0xFFFC00FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x801FF000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithRor_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000FFFF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A00461 // MOV r0, r1, ROR#8
            });

            Assert.Equal((uint)0xFF0000FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000FFFF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithLslZeroImmediate_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x000001FF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A00001 // MOV r0, r1, LSL#0
            });

            Assert.Equal((uint)0x000001FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x000001FF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.False(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithLsrZeroImmediate_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0xFF800000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A00021 // MOV r0, r1, LSR#0
            });

            Assert.Equal((uint)0x00000000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0xFF800000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithAsrZeroImmediate_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x801FF000;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A00041 // MOV r0, r1, ASR#0
            });

            Assert.Equal((uint)0xFFFFFFFF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x801FF000, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithRorZeroImmediate_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x0000FFFF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A00061 // MOV r0, r1, ROR#0
            });

            Assert.Equal((uint)0x00007FFF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x0000FFFF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.False(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveRegOneToRegZeroWithLslByRegTwo_RegZeroCorrectAndCarrySet()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x000001FF;
            cpu.CurrentRegisterSet.GetRegister(2) = 0xAABBCC18; // 24 with garbage in higher bits

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1A00211 // MOV r0, r1, LSL r2
            });

            Assert.Equal((uint)0xFF000000, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x000001FF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0xAABBCC18, cpu.CurrentRegisterSet.GetRegister(2));
            Assert.True(cpu.CurrentStatus.Carry);
        }

        [Fact]
        public void Move_MoveImmediateToRegZero_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE3A00EFF // MOV r0, #0x0FF0
            });

            Assert.Equal((uint)0x00000FF0, cpu.CurrentRegisterSet.GetRegister(0));
        }

        [Fact]
        public void Move_MoveRegZeroToPc_PcCorrectAndSpsrCopiedToCpsr()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentSavedStatus = new ProgramStatus();
            cpu.CurrentSavedStatus.RegisterValue = (uint)0b11111000000000000000000011111111;

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1B0F000 // MOVS PC, r0
            });

            Assert.Equal(0b11111000000000000000000011111111, cpu.CurrentStatus.RegisterValue);
            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(CpuUtil.PC));
            Assert.Equal((uint)0xDEADBEEF, cpu.CurrentRegisterSet.GetRegister(0));
        }     

        #endregion

        #region BIC

        [Fact]
        public void BitClear_BicWithRegOneAndRegTwo_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x000000FF;
            cpu.CurrentRegisterSet.GetRegister(2) = 0xFFFFFF00;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1C10002 // BIC r0, r1, r2
            });

            Assert.Equal((uint)0x000000FF, cpu.CurrentRegisterSet.GetRegister(0));
            Assert.Equal((uint)0x000000FF, cpu.CurrentRegisterSet.GetRegister(1));
            Assert.Equal((uint)0xFFFFFF00, cpu.CurrentRegisterSet.GetRegister(2));
        }

        #endregion

        #region MVN

        [Fact]
        public void MoveNegative_MvnWithRegOne_RegZeroCorrect()
        {
            AgbCpu cpu = CpuUtil.CreateCpu();

            cpu.CurrentRegisterSet.GetRegister(0) = 0xDEADBEEF;
            cpu.CurrentRegisterSet.GetRegister(1) = 0x000000FF;

            CpuUtil.RunCpu(cpu, new uint[]
            {
                0xE1E00001 // MVN r0, r1
            });

            Assert.Equal((uint)0xFFFFFF00, cpu.CurrentRegisterSet.GetRegister(0));
        }

        #endregion

    }
}