using AgbSharp.Core.Cpu.Status;
using Xunit;

namespace AgbSharp.Core.Tests.Cpu
{
    public class ProgramStatus_Tests
    {
        [Fact]
        public void Negative_SetTrue_BoolAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Negative = true;

            Assert.True(status.Negative);
            Assert.Equal((uint)0b10000000000000000000000000000000, status.RegisterValue);
        }

        [Fact]
        public void Zero_SetTrue_BoolAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Zero = true;

            Assert.True(status.Zero);
            Assert.Equal((uint)0b01000000000000000000000000000000, status.RegisterValue);
        }

        [Fact]
        public void Carry_SetTrue_BoolAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Carry = true;

            Assert.True(status.Carry);
            Assert.Equal((uint)0b00100000000000000000000000000000, status.RegisterValue);
        }

        [Fact]
        public void Overflow_SetTrue_BoolAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Overflow = true;

            Assert.True(status.Overflow);
            Assert.Equal((uint)0b00010000000000000000000000000000, status.RegisterValue);
        }

        [Fact]
        public void StickyOverflow_SetTrue_BoolAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.StickyOverflow = true;

            Assert.True(status.StickyOverflow);
            Assert.Equal((uint)0b00001000000000000000000000000000, status.RegisterValue);
        }

        [Fact]
        public void IrqDisable_SetTrue_BoolAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.IrqDisable = true;

            Assert.True(status.IrqDisable);
            Assert.Equal((uint)0b00000000000000000000000010000000, status.RegisterValue);
        }

        [Fact]
        public void FastIrqDisable_SetTrue_BoolAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.FastIrqDisable = true;

            Assert.True(status.FastIrqDisable);
            Assert.Equal((uint)0b00000000000000000000000001000000, status.RegisterValue);
        }

        [Fact]
        public void Thumb_SetTrue_BoolAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Thumb = true;

            Assert.True(status.Thumb);
            Assert.Equal((uint)0b00000000000000000000000000100000, status.RegisterValue);
        }

        [Fact]
        public void CpuMode_SetUser_PropertyAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Mode = CpuMode.User;

            Assert.Equal(CpuMode.User, status.Mode);
            Assert.Equal((uint)0b00000000000000000000000000010000, status.RegisterValue);
        }

        [Fact]
        public void CpuMode_SetFastIrq_PropertyAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Mode = CpuMode.FastIrq;

            Assert.Equal(CpuMode.FastIrq, status.Mode);
            Assert.Equal((uint)0b00000000000000000000000000010001, status.RegisterValue);
        }

        [Fact]
        public void CpuMode_SetIrq_PropertyAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Mode = CpuMode.Irq;

            Assert.Equal(CpuMode.Irq, status.Mode);
            Assert.Equal((uint)0b00000000000000000000000000010010, status.RegisterValue);
        }

        [Fact]
        public void CpuMode_SetSupervisor_PropertyAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Mode = CpuMode.Supervisor;

            Assert.Equal(CpuMode.Supervisor, status.Mode);
            Assert.Equal((uint)0b00000000000000000000000000010011, status.RegisterValue);
        }

        [Fact]
        public void CpuMode_SetAbort_PropertyAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Mode = CpuMode.Abort;

            Assert.Equal(CpuMode.Abort, status.Mode);
            Assert.Equal((uint)0b00000000000000000000000000010111, status.RegisterValue);
        }

        [Fact]
        public void CpuMode_SetUndefined_PropertyAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Mode = CpuMode.Undefined;

            Assert.Equal(CpuMode.Undefined, status.Mode);
            Assert.Equal((uint)0b00000000000000000000000000011011, status.RegisterValue);
        }

        [Fact]
        public void CpuMode_SetSystem_PropertyAndRegisterSetCorrectly()
        {
            ProgramStatus status = new ProgramStatus();

            status.Mode = CpuMode.System;

            Assert.Equal(CpuMode.System, status.Mode);
            Assert.Equal((uint)0b00000000000000000000000000011111, status.RegisterValue);
        }
        
        [Fact]
        public void RegisterValue_AllBoolsSetAndModeSystem_Correct()
        {
            ProgramStatus status = new ProgramStatus();

            status.Negative = true;
            status.Zero = true;
            status.Carry = true;
            status.Overflow = true;
            status.StickyOverflow = true;
            status.IrqDisable = true;
            status.FastIrqDisable = true;
            status.Thumb = true;
            status.Mode = CpuMode.System;

            Assert.Equal((uint)0b11111000000000000000000011111111, status.RegisterValue);
        }

        [Fact]
        public void RegisterValue_SetDirectly_AllPropertiesCorrect()
        {
            ProgramStatus status = new ProgramStatus();

            status.RegisterValue = (uint)0b11111000000000000000000011111111;

            Assert.True(status.Negative);
            Assert.True(status.Zero);
            Assert.True(status.Carry);
            Assert.True(status.Overflow);
            Assert.True(status.StickyOverflow);
            Assert.True(status.IrqDisable);
            Assert.True(status.FastIrqDisable);
            Assert.True(status.Thumb);
            Assert.Equal(CpuMode.System, status.Mode);
            Assert.Equal((uint)0b11111000000000000000000011111111, status.RegisterValue);
        }

    }
}